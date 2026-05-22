using MediatR;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;
using FluentValidation.Results;
using DeepMatch.Application.Features.Swipes.Common;
using DeepMatch.Application.Features.Notifications.Mappers;

namespace DeepMatch.Application.Features.Swipes.Commands.SwipeAnswer;

public class SwipeAnswerCommandHandler : IRequestHandler<SwipeAnswerCommand, SwipeResultDto>
{
    private readonly IAnswerRepository _answers;
    private readonly ISwipeRepository _swipes;
    private readonly IUserRepository _users;
    private readonly IMatchRepository _matches;
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SwipeAnswerCommandHandler> _logger;

    public SwipeAnswerCommandHandler(
        IAnswerRepository answers,
        ISwipeRepository swipes,
        IUserRepository users,
        IMatchRepository matches,
        INotificationRepository notifications,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        ILogger<SwipeAnswerCommandHandler> logger)
    {
        _answers = answers;
        _swipes = swipes;
        _users = users;
        _matches = matches;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<SwipeResultDto> Handle(SwipeAnswerCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var targetAnswer = await _answers.GetByIdWithUserAsync(request.AnswerId, cancellationToken);

        if (targetAnswer == null)
        {
            throw new NotFoundException(nameof(Answer), request.AnswerId);
        }

        if (targetAnswer.UserId == currentUserId)
        {
            throw new ForbiddenException("Нельзя свайпнуть собственный ответ");
        }

        var today = DateTime.UtcNow.Date;
        var swipesToday = await _swipes.CountByUserOnDateAsync(currentUserId, today, cancellationToken);

        var currentUser = await _users.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
        {
            throw new NotFoundException(nameof(User), currentUserId);
        }

        if (!currentUser.CanSwipe(swipesToday))
        {
            throw new DailyLimitExceededException(currentUser.GetDailySwipeLimit());
        }

        var existingSwipe = await _swipes.GetByUserAndAnswerAsync(currentUserId, request.AnswerId, cancellationToken);

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

        _swipes.Add(swipe);

        bool isMatch = false;
        Guid? matchId = null;
        string? matchedUserName = null;
        List<Notification> realtimeNotifications = new();

        if (direction == SwipeDirection.Like)
        {
            targetAnswer.LikesCount++;

            var authorId = targetAnswer.UserId;
            var myAnswerIds = await _answers.GetAnswerIdsByUserIdAsync(currentUserId, cancellationToken);

            var mutualLike = await _swipes.GetMutualLikeAsync(authorId, myAnswerIds, cancellationToken);

            if (mutualLike != null)
            {
                var existingMatch = await _matches.GetByUsersAsync(currentUserId, authorId, cancellationToken);

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

                    _matches.Add(match);
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

                    _notifications.AddRange(realtimeNotifications);
                    _logger.LogInformation("Создан мэтч {MatchId} между {User1Id} и {User2Id}", match.Id, currentUserId, authorId);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var notification in realtimeNotifications)
        {
            await _notificationService.SendNotificationToUserAsync(notification.UserId, NotificationMapper.ToPayload(notification));
        }

        return new SwipeResultDto(isMatch, matchId, matchedUserName);
    }
}
