using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Infrastructure.Messaging.SignalR;

namespace DeepMatch.Infrastructure.Services;

internal class NotificationService : INotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<ChatHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendMatchNotificationAsync(Guid userId, Guid matchId)
    {
        _logger.LogInformation("Отправка уведомления пользователю {UserId} о новом мэтче {MatchId}", userId, matchId);

        await _hubContext.Clients
            .Group(matchId.ToString())
            .SendAsync("MatchCreated", new { matchId, message = "У вас новый мэтч!" });
    }

    public async Task SendMessageNotificationAsync(Guid userId, Guid matchId, Guid messageId, string messagePreview)
    {
        _logger.LogInformation("Отправка уведомления о сообщении {MessageId} в мэтче {MatchId} от пользователя {UserId}", messageId, matchId, userId);

        await _hubContext.Clients
            .Group(matchId.ToString())
            .SendAsync("ReceiveMessage", new { id = messageId, senderId = userId, matchId, content = messagePreview, timestamp = DateTime.UtcNow });
    }

    public async Task SendNotificationToUserAsync(Guid userId, object notification)
    {
        _logger.LogInformation("Отправка realtime-уведомления пользователю {UserId}", userId);

        await _hubContext.Clients
            .User(userId.ToString())
            .SendAsync("NewNotification", notification);
    }
}
