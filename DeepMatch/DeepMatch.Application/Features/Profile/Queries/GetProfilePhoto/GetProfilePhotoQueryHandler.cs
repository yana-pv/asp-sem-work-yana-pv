using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Queries.GetAvatar;

namespace DeepMatch.Application.Features.Profile.Queries.GetProfilePhoto;

public class GetProfilePhotoQueryHandler : IRequestHandler<GetProfilePhotoQuery, AvatarResult?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public GetProfilePhotoQueryHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<AvatarResult?> Handle(GetProfilePhotoQuery request, CancellationToken cancellationToken)
    {
        var photo = await _context.UserPhotos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId, cancellationToken);

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
