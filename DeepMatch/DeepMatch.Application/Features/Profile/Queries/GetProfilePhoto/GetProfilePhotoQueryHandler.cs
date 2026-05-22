using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Queries.GetAvatar;

namespace DeepMatch.Application.Features.Profile.Queries.GetProfilePhoto;

public class GetProfilePhotoQueryHandler : IRequestHandler<GetProfilePhotoQuery, AvatarResult?>
{
    private readonly IUserPhotoRepository _photos;
    private readonly IFileStorageService _fileStorage;

    public GetProfilePhotoQueryHandler(IUserPhotoRepository photos, IFileStorageService fileStorage)
    {
        _photos = photos;
        _fileStorage = fileStorage;
    }

    public async Task<AvatarResult?> Handle(GetProfilePhotoQuery request, CancellationToken cancellationToken)
    {
        var photo = await _photos.GetByIdAsync(request.PhotoId, cancellationToken);

        if (photo == null)
        {
            return null;
        }

        var stream = await _fileStorage.GetFileAsync(photo.FileName);
        if (stream == null)
        {
            return null;
        }

        return new AvatarResult(stream, GetContentType(photo.FileName), photo.FileName);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
