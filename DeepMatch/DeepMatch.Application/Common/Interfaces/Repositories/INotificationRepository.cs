using DeepMatch.Application.Features.Notifications.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task<List<NotificationDto>> GetRecentByUserAsync(Guid userId, int take, CancellationToken cancellationToken);
    Task<List<Notification>> GetUnreadByUserAsync(Guid userId, Guid? notificationId, CancellationToken cancellationToken);
    void Add(Notification notification);
    void AddRange(IEnumerable<Notification> notifications);
}
