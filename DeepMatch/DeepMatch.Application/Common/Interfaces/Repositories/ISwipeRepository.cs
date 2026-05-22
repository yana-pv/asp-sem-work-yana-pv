using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface ISwipeRepository
{
    Task<List<Guid>> GetTargetAnswerIdsBySwiperAsync(Guid swiperUserId, CancellationToken cancellationToken);
    Task<int> CountByUserOnDateAsync(Guid userId, DateTime date, CancellationToken cancellationToken);
    Task<Swipe?> GetByUserAndAnswerAsync(Guid userId, Guid answerId, CancellationToken cancellationToken);
    Task<Swipe?> GetMutualLikeAsync(Guid swiperUserId, IReadOnlyCollection<Guid> targetAnswerIds, CancellationToken cancellationToken);
    void Add(Swipe swipe);
}
