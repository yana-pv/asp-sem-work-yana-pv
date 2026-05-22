using MediatR;
using DeepMatch.Application.Common.Interfaces;

namespace DeepMatch.Application.Features.Profile.Queries.GetAvatar;

public class GetAvatarQueryHandler : IRequestHandler<GetAvatarQuery, AvatarResult?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public GetAvatarQueryHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<AvatarResult?> Handle(GetAvatarQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user?.AvatarUrl == null)
            return null;

        var stream = await _fileStorage.GetFileAsync(user.AvatarUrl);
        if (stream == null)
            return null;

        var contentType = GetContentType(user.AvatarUrl);

        return new AvatarResult(stream, contentType, user.AvatarUrl);
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
