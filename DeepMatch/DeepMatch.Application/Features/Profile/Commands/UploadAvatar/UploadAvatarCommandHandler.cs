using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Profile.Commands.UploadAvatar;

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public UploadAvatarCommandHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var extension = Path.GetExtension(request.FileName);
        var fileName = $"avatars/{userId}{extension}";

        var url = await _fileStorage.UploadFileAsync(fileName, request.FileStream, request.ContentType);

        user.AvatarUrl = url;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return url;
    }
}
