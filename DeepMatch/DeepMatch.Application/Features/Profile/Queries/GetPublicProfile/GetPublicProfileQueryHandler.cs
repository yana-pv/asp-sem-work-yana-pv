using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Profile.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Profile.Queries.GetPublicProfile;

public class GetPublicProfileQueryHandler : IRequestHandler<GetPublicProfileQuery, PublicProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IProfilePhotoUrlService _profilePhotoUrlService;

    public GetPublicProfileQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IProfilePhotoUrlService profilePhotoUrlService)
    {
        _context = context;
        _currentUser = currentUser;
        _profilePhotoUrlService = profilePhotoUrlService;
    }

    public async Task<PublicProfileDto> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var hasMatch = await _context.Matches.AnyAsync(m =>
            (m.User1Id == currentUserId && m.User2Id == request.UserId) ||
            (m.User1Id == request.UserId && m.User2Id == currentUserId),
            cancellationToken);

        if (!hasMatch)
        {
            throw new ForbiddenException("Профиль доступен только после мэтча");
        }

        var user = await _context.Users
            .Include(u => u.Badges)
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

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
