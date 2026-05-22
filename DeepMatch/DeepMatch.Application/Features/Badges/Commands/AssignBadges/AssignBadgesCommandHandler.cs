using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;

namespace DeepMatch.Application.Features.Badges.Commands.AssignBadges;

public class AssignBadgesCommandHandler : IRequestHandler<AssignBadgesCommand>
{
    private readonly IUserRepository _users;
    private readonly IBadgeRepository _badges;
    private readonly IMatchRepository _matches;
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AssignBadgesCommandHandler(
        IUserRepository users,
        IBadgeRepository badges,
        IMatchRepository matches,
        INotificationRepository notifications,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _users = users;
        _badges = badges;
        _matches = matches;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task Handle(AssignBadgesCommand request, CancellationToken cancellationToken)
    {
        var users = await _users.GetUsersForBadgeAssignmentAsync(cancellationToken);
        var badges = await _badges.GetAllAsync(cancellationToken);

        var logicianBadge = badges.FirstOrDefault(b => b.Type == BadgeType.Logician);
        var empathBadge = badges.FirstOrDefault(b => b.Type == BadgeType.Empath);
        var eruditeBadge = badges.FirstOrDefault(b => b.Type == BadgeType.Erudite);
        var debaterBadge = badges.FirstOrDefault(b => b.Type == BadgeType.Debater);
        var activeBadge = badges.FirstOrDefault(b => b.Type == BadgeType.Active);
        var matchmakerBadge = badges.FirstOrDefault(b => b.Type == BadgeType.Matchmaker);
        var realtimeNotifications = new List<Notification>();

        foreach (var user in users)
        {
            var newBadges = 0;

            if (logicianBadge != null && !user.Badges.Any(b => b.Id == logicianBadge.Id))
            {
                var logicCount = user.Answers.Count(a => a.Tags.Contains("логика"));
                if (logicCount >= BusinessRules.Badges.TaggedAnswersRequired)
                {
                    user.Badges.Add(logicianBadge);
                    newBadges++;
                }
            }

            if (empathBadge != null && !user.Badges.Any(b => b.Id == empathBadge.Id))
            {
                var empathCount = user.Answers.Count(a => a.Tags.Contains("эмпатия"));
                if (empathCount >= BusinessRules.Badges.TaggedAnswersRequired)
                {
                    user.Badges.Add(empathBadge);
                    newBadges++;
                }
            }

            if (eruditeBadge != null && !user.Badges.Any(b => b.Id == eruditeBadge.Id))
            {
                var categoriesCount = user.Answers
                    .Select(a => a.Question.Category)
                    .Distinct()
                    .Count();
                if (categoriesCount >= BusinessRules.Badges.DistinctCategoriesRequired)
                {
                    user.Badges.Add(eruditeBadge);
                    newBadges++;
                }
            }

            if (debaterBadge != null && !user.Badges.Any(b => b.Id == debaterBadge.Id))
            {
                var totalLikes = user.Answers.Sum(a => a.LikesCount);
                if (totalLikes >= BusinessRules.Badges.LikesRequired)
                {
                    user.Badges.Add(debaterBadge);
                    newBadges++;
                }
            }

            if (activeBadge != null && !user.Badges.Any(b => b.Id == activeBadge.Id))
            {
                var dates = user.Answers
                    .Select(a => DateOnly.FromDateTime(a.CreatedAt))
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();
                var consecutive = 0;
                var expected = DateOnly.FromDateTime(DateTime.UtcNow);
                foreach (var date in dates)
                {
                    if (date == expected)
                    {
                        consecutive++;
                        expected = expected.AddDays(-1);
                    }
                    else break;
                }
                if (consecutive >= BusinessRules.Badges.ConsecutiveAnswerDaysRequired)
                {
                    user.Badges.Add(activeBadge);
                    newBadges++;
                }
            }

            if (matchmakerBadge != null && !user.Badges.Any(b => b.Id == matchmakerBadge.Id))
            {
                var matchCount = await _matches.CountByUserAsync(user.Id, cancellationToken);
                if (matchCount >= BusinessRules.Badges.MatchesRequired)
                {
                    user.Badges.Add(matchmakerBadge);
                    newBadges++;
                }
            }

            if (newBadges > 0)
            {
                var ratingReward = newBadges * BusinessRules.Badges.RatingReward;
                user.Rating.Increase(ratingReward);

                realtimeNotifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Type = NotificationTypes.Badge,
                    Title = $"Получен новый бейдж! ({newBadges} шт.)",
                    Link = ClientRoutes.Profile,
                    CreatedAt = DateTime.UtcNow
                });

                realtimeNotifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Type = NotificationTypes.Rating,
                    Title = $"Рейтинг повышен на {ratingReward} за новые бейджи",
                    Link = ClientRoutes.Profile,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        _notifications.AddRange(realtimeNotifications);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var notification in realtimeNotifications)
        {
            await _notificationService.SendNotificationToUserAsync(notification.UserId, ToNotificationPayload(notification));
        }
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
