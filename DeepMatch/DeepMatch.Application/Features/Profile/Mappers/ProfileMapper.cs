using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Profile.Mappers;

public static class ProfileMapper
{
    public static ProfileDto ToProfileDto(User user, IProfilePhotoUrlService photoUrlService)
    {
        return new ProfileDto(
            user.Id,
            user.UserName,
            user.Email,
            user.Role,
            user.Bio,
            user.AvatarUrl,
            user.Rating.Value,
            ToBadgeDtoList(user.Badges),
            ToPhotoDtoList(user.Photos, photoUrlService),
            user.Answers
                .OrderByDescending(a => a.CreatedAt)
                .Select(ToMyAnswerDto)
                .ToList());
    }

    public static PublicProfileDto ToPublicProfileDto(User user, IProfilePhotoUrlService photoUrlService)
    {
        return new PublicProfileDto(
            user.Id,
            user.UserName,
            user.Bio,
            user.Rating.Value,
            ToBadgeDtoList(user.Badges),
            ToPhotoDtoList(user.Photos, photoUrlService));
    }

    public static ProfilePhotoDto ToPhotoDto(UserPhoto photo, IProfilePhotoUrlService photoUrlService)
    {
        return new ProfilePhotoDto(photo.Id, photoUrlService.GetProfilePhotoUrl(photo.Id), photo.UploadedAt);
    }

    private static BadgeDto ToBadgeDto(Badge badge)
    {
        return new BadgeDto(badge.Id, badge.Name, badge.Description);
    }

    private static MyAnswerDto ToMyAnswerDto(Answer answer)
    {
        return new MyAnswerDto(
            answer.Id,
            answer.Text,
            answer.Question.Text,
            answer.CreatedAt,
            answer.Tags ?? []);
    }

    private static List<BadgeDto> ToBadgeDtoList(IEnumerable<Badge> badges)
    {
        return badges.Select(ToBadgeDto).ToList();
    }

    private static List<ProfilePhotoDto> ToPhotoDtoList(IEnumerable<UserPhoto> photos, IProfilePhotoUrlService photoUrlService)
    {
        return photos
            .OrderByDescending(p => p.UploadedAt)
            .Select(p => ToPhotoDto(p, photoUrlService))
            .ToList();
    }
}
