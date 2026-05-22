using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Swipes.Common;

namespace DeepMatch.Application.Features.Swipes.Queries.GetSwipesRemaining;

public class GetSwipesRemainingQueryHandler : IRequestHandler<GetSwipesRemainingQuery, SwipesRemainingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSwipesRemainingQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SwipesRemainingDto> Handle(GetSwipesRemainingQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var today = DateTime.UtcNow.Date;

        var used = await _context.Swipes
            .CountAsync(s => s.SwiperUserId == userId && s.SwipedAt.Date == today, cancellationToken);

        var user = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        var limit = user.GetDailySwipeLimit();

        return new SwipesRemainingDto(used, limit, Math.Max(0, limit - used));
    }
}
