namespace DeepMatch.Application.Features.Profile.Common;

public record PublicProfileDto(
    Guid Id,
    string UserName,
    string? Bio,
    int Rating,
    List<BadgeDto> Badges,
    List<ProfilePhotoDto> Photos
);
