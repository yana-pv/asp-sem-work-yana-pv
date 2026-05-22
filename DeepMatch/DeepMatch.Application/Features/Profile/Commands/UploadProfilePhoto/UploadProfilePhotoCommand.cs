using MediatR;
using DeepMatch.Application.Features.Profile.Common;

namespace DeepMatch.Application.Features.Profile.Commands.UploadProfilePhoto;

public record UploadProfilePhotoCommand : IRequest<ProfilePhotoDto>
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}


