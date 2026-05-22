using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Answers.Common;

namespace DeepMatch.Application.Features.Answers.Commands.CreateAnswer;

public class CreateAnswerCommandHandler : IRequestHandler<CreateAnswerCommand, AnswerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreateAnswerCommandHandler> _logger;

    public CreateAnswerCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        ILogger<CreateAnswerCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<AnswerDto> Handle(CreateAnswerCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
        {
            throw new NotFoundException(nameof(Question), request.QuestionId);
        }

        var existingAnswer = await _context.Answers
            .FirstOrDefaultAsync(a => a.UserId == _currentUser.UserId && a.QuestionId == request.QuestionId, cancellationToken);

        if (existingAnswer != null)
        {
            throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
            {
                new("QuestionId", "Вы уже ответили на этот вопрос")
            });
        }

        var answer = new Answer
        {
            Id = Guid.NewGuid(),
            Text = request.Text,
            UserId = _currentUser.UserId,
            QuestionId = request.QuestionId,
            CreatedAt = DateTime.UtcNow,
            Tags = new List<string>()
        };

        _context.Answers.Add(answer);

        var user = await _context.Users.FirstAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        user.Rating.Increase(BusinessRules.Rating.AnswerReward);

        var ratingNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            Type = NotificationTypes.Rating,
            Title = $"Рейтинг повышен на {BusinessRules.Rating.AnswerReward} за новый ответ",
            Link = ClientRoutes.Profile,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(ratingNotification);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Пользователь {UserId} ответил на вопрос {QuestionId}", _currentUser.UserId, request.QuestionId);

        await _notificationService.SendNotificationToUserAsync(_currentUser.UserId, ToNotificationPayload(ratingNotification));

        return new AnswerDto(answer.Id, answer.Text, answer.QuestionId, answer.CreatedAt);
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
