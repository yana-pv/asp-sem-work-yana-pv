using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Common;
using DeepMatch.Application.Features.Profile.Mappers;

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

        return ProfileMapper.ToProfileDto(user, _profilePhotoUrlService);
    }
}
