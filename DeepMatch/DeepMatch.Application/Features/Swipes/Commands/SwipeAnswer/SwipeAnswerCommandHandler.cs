using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;
using FluentValidation.Results;
using DeepMatch.Application.Features.Swipes.Common;

namespace DeepMatch.Application.Features.Swipes.Commands.SwipeAnswer;

public class SwipeAnswerCommandHandler : IRequestHandler<SwipeAnswerCommand, SwipeResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SwipeAnswerCommandHandler> _logger;

    public SwipeAnswerCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        ILogger<SwipeAnswerCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<SwipeResultDto> Handle(SwipeAnswerCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var targetAnswer = await _context.Answers
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

        if (targetAnswer == null)
        {
            throw new NotFoundException(nameof(Answer), request.AnswerId);
        }

        if (targetAnswer.UserId == currentUserId)
        {
            throw new ForbiddenException("Нельзя свайпнуть собственный ответ");
        }

        var today = DateTime.UtcNow.Date;
        var swipesToday = await _context.Swipes
            .CountAsync(s => s.SwiperUserId == currentUserId && s.SwipedAt.Date == today, cancellationToken);

        var currentUser = await _context.Users.FirstAsync(u => u.Id == currentUserId, cancellationToken);
        if (!currentUser.CanSwipe(swipesToday))
        {
            throw new DailyLimitExceededException(currentUser.GetDailySwipeLimit());
        }

        var existingSwipe = await _context.Swipes
            .FirstOrDefaultAsync(s => s.SwiperUserId == currentUserId && s.TargetAnswerId == request.AnswerId, cancellationToken);

        if (existingSwipe != null)
        {
            throw new ValidationException(new List<ValidationFailure>
            {
                new("AnswerId", "Вы уже свайпнули этот ответ")
            });
        }

        var direction = request.Direction == "like" ? SwipeDirection.Like : SwipeDirection.Pass;
        var swipe = new Swipe
        {
            Id = Guid.NewGuid(),
            SwiperUserId = currentUserId,
            TargetAnswerId = request.AnswerId,
            Direction = direction,
            SwipedAt = DateTime.UtcNow
        };

        _context.Swipes.Add(swipe);

        bool isMatch = false;
        Guid? matchId = null;
        string? matchedUserName = null;
        List<Notification> realtimeNotifications = new();

        if (direction == SwipeDirection.Like)
        {
            targetAnswer.LikesCount++;

            var authorId = targetAnswer.UserId;
            var myAnswerIds = await _context.Answers
                .Where(a => a.UserId == currentUserId)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            var mutualLike = await _context.Swipes
                .FirstOrDefaultAsync(s =>
                    s.SwiperUserId == authorId &&
                    myAnswerIds.Contains(s.TargetAnswerId) &&
                    s.Direction == SwipeDirection.Like,
                    cancellationToken);

            if (mutualLike != null)
            {
                var existingMatch = await _context.Matches
                    .FirstOrDefaultAsync(m =>
                        (m.User1Id == currentUserId && m.User2Id == authorId) ||
                        (m.User1Id == authorId && m.User2Id == currentUserId),
                        cancellationToken);

                if (existingMatch != null)
                {
                    matchId = existingMatch.Id;
                    matchedUserName = targetAnswer.User.UserName;
                    _logger.LogInformation(
                        "Мэтч между {User1Id} и {User2Id} уже существует: {MatchId}",
                        currentUserId,
                        authorId,
                        existingMatch.Id);
                }
                else
                {
                    var match = new Match
                    {
                        Id = Guid.NewGuid(),
                        User1Id = currentUserId,
                        User2Id = authorId,
                        CatalystAnswer1Id = mutualLike.TargetAnswerId,
                        CatalystAnswer2Id = request.AnswerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Matches.Add(match);
                    isMatch = true;
                    matchId = match.Id;
                    matchedUserName = targetAnswer.User.UserName;

                    currentUser.Rating.Increase(BusinessRules.Rating.MatchReward);
                    targetAnswer.User.Rating.Increase(BusinessRules.Rating.MatchReward);

                    realtimeNotifications.AddRange(new[]
                    {
                        new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = currentUserId,
                            Type = NotificationTypes.Match,
                            Title = $"У вас мэтч с {targetAnswer.User.UserName}!",
                            Link = ClientRoutes.Matches,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = authorId,
                            Type = NotificationTypes.Match,
                            Title = $"У вас мэтч с {currentUser.UserName}!",
                            Link = ClientRoutes.Matches,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = currentUserId,
                            Type = NotificationTypes.Rating,
                            Title = $"Рейтинг повышен на {BusinessRules.Rating.MatchReward} за новый мэтч",
                            Link = ClientRoutes.Profile,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = authorId,
                            Type = NotificationTypes.Rating,
                            Title = $"Рейтинг повышен на {BusinessRules.Rating.MatchReward} за новый мэтч",
                            Link = ClientRoutes.Profile,
                            CreatedAt = DateTime.UtcNow
                        }
                    });

                    _context.Notifications.AddRange(realtimeNotifications);
                    _logger.LogInformation("Создан мэтч {MatchId} между {User1Id} и {User2Id}", match.Id, currentUserId, authorId);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var notification in realtimeNotifications)
        {
            await _notificationService.SendNotificationToUserAsync(notification.UserId, ToNotificationPayload(notification));
        }

        return new SwipeResultDto(isMatch, matchId, matchedUserName);
    }

    private static object ToNotificationPayload(Notification notification) => new
    {
        notification.Id,
        notification.Type,
        notification.Title,
        notification.Link,
        notification.IsRead,
        notification.CreatedAt
    };
}
