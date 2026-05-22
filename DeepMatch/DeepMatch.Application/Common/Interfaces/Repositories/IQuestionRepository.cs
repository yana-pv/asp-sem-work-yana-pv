using DeepMatch.Application.Features.Questions.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Question?> GetRandomUnassignedActiveAsync(CancellationToken cancellationToken);
    Task<List<Question>> GetQuestionsAssignedForDateAsync(DateOnly date, CancellationToken cancellationToken);
    Task<List<QuestionAdminDto>> GetAllAdminAsync(CancellationToken cancellationToken);
    Task<DailyQuestionDto?> GetDailyQuestionAsync(DateOnly date, CancellationToken cancellationToken);
    Task<int> CountActiveAsync(CancellationToken cancellationToken);
    void Add(Question question);
}
