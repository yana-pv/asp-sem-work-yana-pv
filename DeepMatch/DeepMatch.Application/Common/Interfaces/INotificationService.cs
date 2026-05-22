using DeepMatch.Application.Features.Notifications.Common;

namespace DeepMatch.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendMatchNotificationAsync(Guid userId, Guid matchId);
    Task SendMessageNotificationAsync(Guid userId, Guid matchId, Guid messageId, string messagePreview);
    Task SendNotificationToUserAsync(Guid userId, NotificationPayload notification);
}
