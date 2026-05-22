using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Questions.Commands.AssignDailyQuestion;

public class AssignDailyQuestionCommandHandler : IRequestHandler<AssignDailyQuestionCommand>
{
    private readonly IQuestionRepository _questions;
    private readonly IUserRepository _users;
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AssignDailyQuestionCommandHandler(
        IQuestionRepository questions,
        IUserRepository users,
        INotificationRepository notifications,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _questions = questions;
        _users = users;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task Handle(AssignDailyQuestionCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var oldQuestions = await _questions.GetQuestionsAssignedForDateAsync(today, cancellationToken);
        foreach (var q in oldQuestions)
        {
            q.DateOfDay = null;
        }

        var newQuestion = await _questions.GetRandomUnassignedActiveAsync(cancellationToken);

        if (newQuestion == null)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        newQuestion.DateOfDay = today;

        var userIds = await _users.GetActiveUserIdsAsync(cancellationToken);

        var notifications = userIds.Select(userId => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = NotificationTypes.Question,
            Title = "Новый вопрос дня уже доступен",
            Link = ClientRoutes.Daily,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _notifications.AddRange(notifications);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
