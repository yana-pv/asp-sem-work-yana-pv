using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Auth.Common;

namespace DeepMatch.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashService _passwordHashService;

    public LoginCommandHandler(IApplicationDbContext context, IPasswordHashService passwordHashService)
    {
        _context = context;
        _passwordHashService = passwordHashService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

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
