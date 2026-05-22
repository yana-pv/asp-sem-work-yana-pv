namespace DeepMatch.Application.Features.Notifications.Common;

public record NotificationPayload(
    Guid Id,
    string Type,
    string Title,
    string? Link,
    bool IsRead,
    DateTime CreatedAt);
