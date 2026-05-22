using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Chat.Common;

namespace DeepMatch.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public SendMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

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

        _context.Messages.Add(message);
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.SendMessageNotificationAsync(currentUserId, request.MatchId, message.Id, request.Content);
        await _notificationService.SendNotificationToUserAsync(otherUserId, ToNotificationPayload(notification));

        return new MessageDto(message.Id, message.MatchId, message.Content, message.SenderUserId, message.Timestamp, message.IsIcebreaker);
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
