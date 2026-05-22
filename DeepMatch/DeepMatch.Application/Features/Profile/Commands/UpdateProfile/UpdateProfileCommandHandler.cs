using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateProfileCommandHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(_currentUser.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), _currentUser.UserId);
        }

        user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
