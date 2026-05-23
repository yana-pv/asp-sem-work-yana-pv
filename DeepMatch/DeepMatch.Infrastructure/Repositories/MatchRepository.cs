using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Matches.Common;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _context;

    public MatchRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Matches.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public Task<Match?> GetByIdWithChatInfoAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Matches
            .Include(m => m.User1)
            .Include(m => m.User2)
            .Include(m => m.CatalystAnswer1)
                .ThenInclude(a => a.Question)
            .Include(m => m.CatalystAnswer2)
                .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public Task<Match?> GetByUsersAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken)
    {
        return _context.Matches
            .FirstOrDefaultAsync(m =>
                (m.User1Id == user1Id && m.User2Id == user2Id) ||
                (m.User1Id == user2Id && m.User2Id == user1Id),
                cancellationToken);
    }

    public Task<List<MatchDto>> GetMatchesForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Matches
            .Where(m => m.User1Id == userId || m.User2Id == userId)
            .Select(m => new
            {
                Match = m,
                MatchedUserName = m.User1Id == userId ? m.User2.UserName : m.User1.UserName,
                MatchedUserId = m.User1Id == userId ? m.User2Id : m.User1Id,
                LastActivityAt = _context.Messages
                    .Where(message => message.MatchId == m.Id)
                    .Select(message => (DateTime?)message.Timestamp)
                    .Max() ?? m.CreatedAt,
                HasUnreadMessages = _context.Messages
                    .Any(message =>
                        message.MatchId == m.Id &&
                        message.SenderUserId != userId &&
                        !message.IsRead)
            })
            .OrderByDescending(m => m.LastActivityAt)
            .Select(m => new MatchDto(
                m.Match.Id,
                m.MatchedUserName,
                m.MatchedUserId,
                m.LastActivityAt,
                m.HasUnreadMessages))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> UsersHaveMatchAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken)
    {
        return _context.Matches.AnyAsync(m =>
            (m.User1Id == user1Id && m.User2Id == user2Id) ||
            (m.User1Id == user2Id && m.User2Id == user1Id),
            cancellationToken);
    }

    public Task<bool> MatchInvolvesUserAsync(Guid matchId, Guid userId, CancellationToken cancellationToken)
    {
        return _context.Matches.AnyAsync(
            m => m.Id == matchId && (m.User1Id == userId || m.User2Id == userId),
            cancellationToken);
    }

    public Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Matches.CountAsync(m => m.User1Id == userId || m.User2Id == userId, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return _context.Matches.CountAsync(cancellationToken);
    }

    public void Add(Match match)
    {
        _context.Matches.Add(match);
    }
}
