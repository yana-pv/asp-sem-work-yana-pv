using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Questions.Commands.AssignDailyQuestion;

public class AssignDailyQuestionCommandHandler : IRequestHandler<AssignDailyQuestionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public AssignDailyQuestionCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(AssignDailyQuestionCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var oldQuestions = await _context.Questions
            .Where(q => q.DateOfDay.HasValue && q.DateOfDay == today)
            .ToListAsync(cancellationToken);

        foreach (var q in oldQuestions)
        {
            q.DateOfDay = null;
        }

        var newQuestion = await _context.Questions
            .Where(q => q.IsActive && !q.DateOfDay.HasValue)
            .OrderBy(q => EF.Functions.Random())
            .FirstOrDefaultAsync(cancellationToken);

        if (newQuestion == null)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        newQuestion.DateOfDay = today;

        var users = await _context.Users
            .Where(u => !u.IsBlocked)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var notifications = users.Select(userId => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = NotificationTypes.Question,
            Title = "Новый вопрос дня уже доступен",
            Link = ClientRoutes.Daily,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var notification in notifications)
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
