using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Auth.Common;

namespace DeepMatch.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _passwordHashService;

    public RegisterCommandHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHashService passwordHashService)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHashService = passwordHashService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _users.GetByEmailAsync(request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new Application.Common.Exceptions.ValidationException(new List<FluentValidation.Results.ValidationFailure>
            {
                new("Email", "Пользователь с таким email уже существует")
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.UserName,
            Age = request.Age,
            Role = UserRoles.User,
            RegisteredAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHashService.HashPassword(user, request.Password);

        _users.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(user.Id, user.Email, user.UserName, user.Role);
    }
}
