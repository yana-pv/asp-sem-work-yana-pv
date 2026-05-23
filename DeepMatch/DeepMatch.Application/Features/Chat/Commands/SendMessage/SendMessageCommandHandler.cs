using MediatR;
using DeepMatch.Application.Common.Constants;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Chat.Common;
using DeepMatch.Application.Features.Notifications.Mappers;

namespace DeepMatch.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public SendMessageCommandHandler(
        IMatchRepository matches,
        IMessageRepository messages,
        INotificationRepository notifications,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _matches = matches;
        _messages = messages;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(Match), request.MatchId);
        }

        if (!match.InvolvesUser(currentUserId))
        {
            throw new ForbiddenException("Вы не участвуете в этом мэтче");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            MatchId = request.MatchId,
            SenderUserId = currentUserId,
            Content = request.Content,
            Timestamp = DateTime.UtcNow
        };

        var otherUserId = match.GetOtherUserId(currentUserId);
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Type = NotificationTypes.Message,
            Title = "Новое сообщение в чате",
            Link = ClientRoutes.Match(request.MatchId),
            CreatedAt = DateTime.UtcNow
        };

        _messages.Add(message);
        _notifications.Add(notification);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.SendMessageNotificationAsync(currentUserId, request.MatchId, message.Id, request.Content);
        await _notificationService.SendNotificationToUserAsync(otherUserId, NotificationMapper.ToPayload(notification));

        return new MessageDto(
            message.Id,
            message.MatchId,
            message.Content,
            message.SenderUserId,
            message.Timestamp,
            message.IsIcebreaker);
    }
}
