using MediatR;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Constants;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Answers.Common;
using DeepMatch.Application.Features.Notifications.Mappers;

namespace DeepMatch.Application.Features.Answers.Commands.CreateAnswer;

public class CreateAnswerCommandHandler : IRequestHandler<CreateAnswerCommand, AnswerDto>
{
    private readonly IQuestionRepository _questions;
    private readonly IAnswerRepository _answers;
    private readonly IUserRepository _users;
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreateAnswerCommandHandler> _logger;

    public CreateAnswerCommandHandler(
        IQuestionRepository questions,
        IAnswerRepository answers,
        IUserRepository users,
        INotificationRepository notifications,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        ILogger<CreateAnswerCommandHandler> logger)
    {
        _questions = questions;
        _answers = answers;
        _users = users;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<AnswerDto> Handle(CreateAnswerCommand request, CancellationToken cancellationToken)
    {
        var question = await _questions.GetByIdAsync(request.QuestionId, cancellationToken);

        if (question == null)
        {
            throw new NotFoundException(nameof(Question), request.QuestionId);
        }

        var existingAnswer = await _answers.GetByUserAndQuestionAsync(_currentUser.UserId, request.QuestionId, cancellationToken);

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

        _answers.Add(answer);

        var user = await _users.GetByIdAsync(_currentUser.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), _currentUser.UserId);
        }

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
        _notifications.Add(ratingNotification);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Пользователь {UserId} ответил на вопрос {QuestionId}", _currentUser.UserId, request.QuestionId);

        await _notificationService.SendNotificationToUserAsync(_currentUser.UserId, NotificationMapper.ToPayload(ratingNotification));

        return new AnswerDto(answer.Id, answer.Text, answer.QuestionId, answer.CreatedAt);
    }
}
