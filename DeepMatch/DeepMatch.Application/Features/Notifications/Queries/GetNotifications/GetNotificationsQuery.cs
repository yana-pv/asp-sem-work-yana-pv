using MediatR;
using DeepMatch.Application.Features.Notifications.Common;

namespace DeepMatch.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery : IRequest<List<NotificationDto>>;
