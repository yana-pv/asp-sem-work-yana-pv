using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Auth.Common;

namespace DeepMatch.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHashService _passwordHashService;

    public LoginCommandHandler(IUserRepository users, IPasswordHashService passwordHashService)
    {
        _users = users;
        _passwordHashService = passwordHashService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            throw new Application.Common.Exceptions.ValidationException(new List<FluentValidation.Results.ValidationFailure>
            {
                new("Email", "Неверный email или пароль")
            });
        }

        if (!_passwordHashService.VerifyPassword(user, request.Password))
        {
            throw new Application.Common.Exceptions.ValidationException(new List<FluentValidation.Results.ValidationFailure>
            {
                new("Email", "Неверный email или пароль")
            });
        }

        if (user.IsBlocked)
        {
            throw new Application.Common.Exceptions.ForbiddenException("Ваш аккаунт заблокирован");
        }

        return new AuthResponseDto(user.Id, user.Email, user.UserName, user.Role);
    }
}
