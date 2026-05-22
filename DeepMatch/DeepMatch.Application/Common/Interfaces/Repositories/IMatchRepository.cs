using DeepMatch.Application.Features.Matches.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Match?> GetByIdWithChatInfoAsync(Guid id, CancellationToken cancellationToken);
    Task<Match?> GetByUsersAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken);
    Task<List<MatchDto>> GetMatchesForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> UsersHaveMatchAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken);
    Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    void Add(Match match);
}
