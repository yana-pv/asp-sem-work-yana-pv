using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Application.Features.Notifications.Common;

namespace DeepMatch.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    private readonly INotificationRepository _notifications;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(INotificationRepository notifications, ICurrentUserService currentUser)
    {
        _notifications = notifications;
        _currentUser = currentUser;
    }

    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await _notifications.GetRecentByUserAsync(
            _currentUser.UserId,
            BusinessRules.Notifications.PageSize,
            cancellationToken);
    }
}
