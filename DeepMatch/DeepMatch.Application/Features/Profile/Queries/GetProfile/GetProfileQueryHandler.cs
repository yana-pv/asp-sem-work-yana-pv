using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Common;

namespace DeepMatch.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto?>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;
    private readonly IProfilePhotoUrlService _profilePhotoUrlService;

    public GetProfileQueryHandler(
        IUserRepository users,
        ICurrentUserService currentUser,
        IProfilePhotoUrlService profilePhotoUrlService)
    {
        _users = users;
        _currentUser = currentUser;
        _profilePhotoUrlService = profilePhotoUrlService;
    }

    public async Task<ProfileDto?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetProfileAsync(_currentUser.UserId, cancellationToken);

        if (user == null) return null;

        return new ProfileDto(
            user.Id,
            user.UserName,
            user.Email,
            user.Role,
            user.Bio,
            user.AvatarUrl,
            user.Rating.Value,
            user.Badges.Select(b => new BadgeDto(b.Id, b.Name, b.Description)).ToList(),
            user.Photos
                .OrderByDescending(p => p.UploadedAt)
                .Select(p => new ProfilePhotoDto(p.Id, _profilePhotoUrlService.GetProfilePhotoUrl(p.Id), p.UploadedAt))
                .ToList(),
            user.Answers.OrderByDescending(a => a.CreatedAt).Select(a =>
                new MyAnswerDto(a.Id, a.Text, a.Question.Text, a.CreatedAt, a.Tags ?? new List<string>())).ToList()
        );
    }
}
