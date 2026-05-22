using DeepMatch.Application.Features.Notifications.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Notifications.Mappers;

public static class NotificationMapper
{
    public static NotificationPayload ToPayload(Notification notification)
    {
        return new NotificationPayload(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Link,
            notification.IsRead,
            notification.CreatedAt);
    }
}
