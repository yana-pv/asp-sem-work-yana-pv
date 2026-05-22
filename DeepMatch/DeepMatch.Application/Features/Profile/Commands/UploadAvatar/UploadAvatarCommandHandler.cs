using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;

namespace DeepMatch.Application.Features.Profile.Commands.UploadAvatar;


public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public UploadAvatarCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var user = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        var extension = Path.GetExtension(request.FileName);
        var fileName = $"avatars/{userId}{extension}";

        var url = await _fileStorage.UploadFileAsync(fileName, request.FileStream, request.ContentType);

        user.AvatarUrl = url;
        await _context.SaveChangesAsync(cancellationToken);

        return url;
    }
}