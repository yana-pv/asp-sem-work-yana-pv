using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Swipes.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Swipes.Queries.GetSwipesRemaining;

public class GetSwipesRemainingQueryHandler : IRequestHandler<GetSwipesRemainingQuery, SwipesRemainingDto>
{
    private readonly ISwipeRepository _swipes;
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;

    public GetSwipesRemainingQueryHandler(
        ISwipeRepository swipes,
        IUserRepository users,
        ICurrentUserService currentUser)
    {
        _swipes = swipes;
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<SwipesRemainingDto> Handle(GetSwipesRemainingQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var today = DateTime.UtcNow.Date;

        var used = await _swipes.CountByUserOnDateAsync(userId, today, cancellationToken);
        var user = await _users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var limit = user.GetDailySwipeLimit();

        return new SwipesRemainingDto(used, limit, Math.Max(0, limit - used));
    }
}
