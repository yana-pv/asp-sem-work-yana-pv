namespace DeepMatch.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendMatchNotificationAsync(Guid userId, Guid matchId);
    Task SendMessageNotificationAsync(Guid userId, Guid matchId, Guid messageId, string messagePreview);
    Task SendNotificationToUserAsync(Guid userId, object notification);
}
