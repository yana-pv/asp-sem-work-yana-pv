using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Profile.Commands.UploadProfilePhoto;

public class UploadProfilePhotoCommandHandler : IRequestHandler<UploadProfilePhotoCommand, ProfilePhotoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IProfilePhotoUrlService _profilePhotoUrlService;

    public UploadProfilePhotoCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage,
        IProfilePhotoUrlService profilePhotoUrlService)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _profilePhotoUrlService = profilePhotoUrlService;
    }

    public async Task<ProfilePhotoDto> Handle(UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        var extension = Path.GetExtension(request.FileName);
        var photo = new UserPhoto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UploadedAt = DateTime.UtcNow
        };

        var fileName = $"photos/{userId}/{photo.Id}{extension}";
        photo.FileName = await _fileStorage.UploadFileAsync(fileName, request.FileStream, request.ContentType);

        _context.UserPhotos.Add(photo);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProfilePhotoDto(photo.Id, _profilePhotoUrlService.GetProfilePhotoUrl(photo.Id), photo.UploadedAt);
    }
}
