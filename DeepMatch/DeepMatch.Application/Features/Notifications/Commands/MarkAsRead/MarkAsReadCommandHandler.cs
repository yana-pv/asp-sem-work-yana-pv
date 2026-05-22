using MediatR;
using DeepMatch.Application.Common.Interfaces;

namespace DeepMatch.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkAsReadCommandHandler(
        INotificationRepository notifications,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notifications = await _notifications.GetUnreadByUserAsync(
            _currentUser.UserId,
            request.NotificationId,
            cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
