using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class SwipeRepository : ISwipeRepository
{
    private readonly AppDbContext _context;

    public SwipeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Guid>> GetTargetAnswerIdsBySwiperAsync(Guid swiperUserId, CancellationToken cancellationToken)
    {
        return _context.Swipes
            .Where(s => s.SwiperUserId == swiperUserId)
            .Select(s => s.TargetAnswerId)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByUserOnDateAsync(Guid userId, DateTime date, CancellationToken cancellationToken)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        return _context.Swipes
            .CountAsync(s => s.SwiperUserId == userId && s.SwipedAt >= start && s.SwipedAt < end, cancellationToken);
    }

    public Task<Swipe?> GetByUserAndAnswerAsync(Guid userId, Guid answerId, CancellationToken cancellationToken)
    {
        return _context.Swipes
            .FirstOrDefaultAsync(s => s.SwiperUserId == userId && s.TargetAnswerId == answerId, cancellationToken);
    }

    public Task<Swipe?> GetMutualLikeAsync(Guid swiperUserId, IReadOnlyCollection<Guid> targetAnswerIds, CancellationToken cancellationToken)
    {
        return _context.Swipes
            .FirstOrDefaultAsync(s =>
                s.SwiperUserId == swiperUserId &&
                targetAnswerIds.Contains(s.TargetAnswerId) &&
                s.Direction == SwipeDirection.Like,
                cancellationToken);
    }

    public void Add(Swipe swipe)
    {
        _context.Swipes.Add(swipe);
    }
}
