using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;

namespace DeepMatch.Application.Features.Badges.Commands.AssignBadges;

public record AssignBadgesCommand : IRequest;

public class AssignBadgesCommandHandler : IRequestHandler<AssignBadgesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public AssignBadgesCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(AssignBadgesCommand request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.Answers)
                .ThenInclude(a => a.Question)
            .Include(u => u.Badges)
            .ToListAsync(cancellationToken);

        var badges = await _context.Badges.ToListAsync(cancellationToken);

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

            // Логик
            if (logicianBadge != null && !user.Badges.Any(b => b.Id == logicianBadge.Id))
            {
                var logicCount = user.Answers.Count(a => a.Tags.Contains("логика"));
                if (logicCount >= BusinessRules.Badges.TaggedAnswersRequired)
                {
                    user.Badges.Add(logicianBadge);
                    newBadges++;
                }
            }

            // Эмпат
            if (empathBadge != null && !user.Badges.Any(b => b.Id == empathBadge.Id))
            {
                var empathCount = user.Answers.Count(a => a.Tags.Contains("эмпатия"));
                if (empathCount >= BusinessRules.Badges.TaggedAnswersRequired)
                {
                    user.Badges.Add(empathBadge);
                    newBadges++;
                }
            }

            // Эрудит
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

            // Мастер дебатов
            if (debaterBadge != null && !user.Badges.Any(b => b.Id == debaterBadge.Id))
            {
                var totalLikes = user.Answers.Sum(a => a.LikesCount);
                if (totalLikes >= BusinessRules.Badges.LikesRequired)
                {
                    user.Badges.Add(debaterBadge);
                    newBadges++;
                }
            }

            // Активный участник
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

            // Создатель связей
            if (matchmakerBadge != null && !user.Badges.Any(b => b.Id == matchmakerBadge.Id))
            {
                var matchCount = await _context.Matches
                    .CountAsync(m => m.User1Id == user.Id || m.User2Id == user.Id, cancellationToken);
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

        _context.Notifications.AddRange(realtimeNotifications);
        await _context.SaveChangesAsync(cancellationToken);

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
