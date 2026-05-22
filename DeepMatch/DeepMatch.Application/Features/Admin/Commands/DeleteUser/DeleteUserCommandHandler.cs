using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Admin.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUser.UserId)
        {
            throw new ForbiddenException("Нельзя заблокировать самого себя");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

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

        await _context.SaveChangesAsync(cancellationToken);
    }
}
