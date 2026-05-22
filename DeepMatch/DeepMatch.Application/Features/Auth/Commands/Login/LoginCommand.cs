using MediatR;
using DeepMatch.Application.Features.Auth.Common;

namespace DeepMatch.Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<AuthResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
