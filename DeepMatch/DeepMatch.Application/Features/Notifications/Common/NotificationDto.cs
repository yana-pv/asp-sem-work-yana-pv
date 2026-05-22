namespace DeepMatch.Application.Features.Notifications.Common;

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string? Link,
    bool IsRead,
    DateTime CreatedAt
);
