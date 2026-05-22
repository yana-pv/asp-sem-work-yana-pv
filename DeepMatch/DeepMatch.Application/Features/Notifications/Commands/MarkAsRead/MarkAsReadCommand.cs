using MediatR;

namespace DeepMatch.Application.Features.Notifications.Commands.MarkAsRead;

public record MarkAsReadCommand(Guid? NotificationId) : IRequest;


