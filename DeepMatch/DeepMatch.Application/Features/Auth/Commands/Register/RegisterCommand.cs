using MediatR;
using DeepMatch.Application.Features.Auth.Common;

namespace DeepMatch.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<AuthResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int Age { get; init; }
}
