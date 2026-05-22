using MediatR;

namespace DeepMatch.Application.Features.Admin.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId, string? Reason) : IRequest;


