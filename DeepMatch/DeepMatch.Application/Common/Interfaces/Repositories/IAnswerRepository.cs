using DeepMatch.Application.Features.Answers.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IAnswerRepository
{
    Task<Answer?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken);
    Task<Answer?> GetByUserAndQuestionAsync(Guid userId, Guid questionId, CancellationToken cancellationToken);
    Task<List<Guid>> GetAnswerIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<Answer>> GetUnprocessedBatchAsync(int batchSize, CancellationToken cancellationToken);
    Task<FeedCardDto?> GetRandomFeedCardAsync(Guid currentUserId, IReadOnlyCollection<Guid> swipedAnswerIds, CancellationToken cancellationToken);
    Task<List<Answer>> GetAnswersByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    void Add(Answer answer);
}
