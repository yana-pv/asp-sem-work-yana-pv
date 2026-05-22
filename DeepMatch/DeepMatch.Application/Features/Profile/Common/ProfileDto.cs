namespace DeepMatch.Application.Features.Profile.Common;

public record ProfileDto(
    Guid Id,
    string UserName,
    string Email,
    string Role,
    string? Bio,
    string? AvatarUrl,
    int Rating,
    List<BadgeDto> Badges,
    List<ProfilePhotoDto> Photos,
    List<MyAnswerDto> Answers
);
