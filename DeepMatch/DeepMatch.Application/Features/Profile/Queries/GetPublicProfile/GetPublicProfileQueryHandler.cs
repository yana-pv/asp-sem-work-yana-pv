using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Profile.Queries.GetPublicProfile;

public class GetPublicProfileQueryHandler : IRequestHandler<GetPublicProfileQuery, PublicProfileDto>
{
    private readonly IUserRepository _users;
    private readonly IMatchRepository _matches;
    private readonly ICurrentUserService _currentUser;
    private readonly IProfilePhotoUrlService _profilePhotoUrlService;

    public GetPublicProfileQueryHandler(
        IUserRepository users,
        IMatchRepository matches,
        ICurrentUserService currentUser,
        IProfilePhotoUrlService profilePhotoUrlService)
    {
        _users = users;
        _matches = matches;
        _currentUser = currentUser;
        _profilePhotoUrlService = profilePhotoUrlService;
    }

    public async Task<PublicProfileDto> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var hasMatch = await _matches.UsersHaveMatchAsync(currentUserId, request.UserId, cancellationToken);

        if (!hasMatch)
        {
            throw new ForbiddenException("Профиль доступен только после мэтча");
        }

        var user = await _users.GetPublicProfileAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        return new PublicProfileDto(
            user.Id,
            user.UserName,
            user.Bio,
            user.Rating.Value,
            user.Badges.Select(b => new BadgeDto(b.Id, b.Name, b.Description)).ToList(),
            user.Photos
                .OrderByDescending(p => p.UploadedAt)
                .Select(p => new ProfilePhotoDto(p.Id, _profilePhotoUrlService.GetProfilePhotoUrl(p.Id), p.UploadedAt))
                .ToList());
    }
}
