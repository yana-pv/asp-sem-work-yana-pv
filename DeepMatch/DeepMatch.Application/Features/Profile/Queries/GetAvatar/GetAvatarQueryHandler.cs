using MediatR;
using DeepMatch.Application.Common.Interfaces;

namespace DeepMatch.Application.Features.Profile.Queries.GetAvatar;

public class GetAvatarQueryHandler : IRequestHandler<GetAvatarQuery, AvatarResult?>
{
    private readonly IUserRepository _users;
    private readonly IFileStorageService _fileStorage;

    public GetAvatarQueryHandler(IUserRepository users, IFileStorageService fileStorage)
    {
        _users = users;
        _fileStorage = fileStorage;
    }

    public async Task<AvatarResult?> Handle(GetAvatarQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

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
