using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Admin.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUser.UserId)
        {
            throw new ForbiddenException("Нельзя заблокировать самого себя");
        }

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        if (user.Role is UserRoles.Admin or UserRoles.System)
        {
            throw new ForbiddenException("Нельзя заблокировать администратора или системного пользователя");
        }

        user.IsBlocked = true;
        user.BlockReason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Блокировка администратором"
            : request.Reason.Trim();
        user.BlockedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
