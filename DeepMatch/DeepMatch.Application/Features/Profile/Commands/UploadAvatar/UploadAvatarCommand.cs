using MediatR;

namespace DeepMatch.Application.Features.Profile.Commands.UploadAvatar;

public record UploadAvatarCommand : IRequest<string>
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

