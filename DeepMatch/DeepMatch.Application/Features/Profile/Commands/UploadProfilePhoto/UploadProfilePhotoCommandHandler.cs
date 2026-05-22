using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Common;
using DeepMatch.Application.Features.Profile.Mappers;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Profile.Commands.UploadProfilePhoto;

public class UploadProfilePhotoCommandHandler : IRequestHandler<UploadProfilePhotoCommand, ProfilePhotoDto>
{
    private readonly IUserRepository _users;
    private readonly IUserPhotoRepository _photos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IProfilePhotoUrlService _profilePhotoUrlService;

    public UploadProfilePhotoCommandHandler(
        IUserRepository users,
        IUserPhotoRepository photos,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage,
        IProfilePhotoUrlService profilePhotoUrlService)
    {
        _users = users;
        _photos = photos;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _profilePhotoUrlService = profilePhotoUrlService;
    }

    public async Task<ProfilePhotoDto> Handle(UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var extension = Path.GetExtension(request.FileName);
        var photo = new UserPhoto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UploadedAt = DateTime.UtcNow
        };

        var fileName = $"photos/{userId}/{photo.Id}{extension}";
        photo.FileName = await _fileStorage.UploadFileAsync(fileName, request.FileStream, request.ContentType);

        _photos.Add(photo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProfileMapper.ToPhotoDto(photo, _profilePhotoUrlService);
    }
}
