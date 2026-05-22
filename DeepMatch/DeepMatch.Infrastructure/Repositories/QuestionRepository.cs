using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Questions.Common;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;

    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Questions.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public Task<Question?> GetRandomUnassignedActiveAsync(CancellationToken cancellationToken)
    {
        return _context.Questions
            .Where(q => q.IsActive && !q.DateOfDay.HasValue)
            .OrderBy(q => EF.Functions.Random())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<Question>> GetQuestionsAssignedForDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        return _context.Questions
            .Where(q => q.DateOfDay.HasValue && q.DateOfDay == date)
            .ToListAsync(cancellationToken);
    }

    public Task<List<QuestionAdminDto>> GetAllAdminAsync(CancellationToken cancellationToken)
    {
        return _context.Questions
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuestionAdminDto(
                q.Id,
                q.Text,
                q.Category.ToString(),
                q.DateOfDay,
                q.IsActive,
                q.Answers.Count,
                q.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<DailyQuestionDto?> GetDailyQuestionAsync(DateOnly date, CancellationToken cancellationToken)
    {
        return _context.Questions
            .Where(q => q.DateOfDay == date && q.IsActive)
            .Select(q => new DailyQuestionDto(q.Id, q.Text, q.Category.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountActiveAsync(CancellationToken cancellationToken)
    {
        return _context.Questions.CountAsync(q => q.IsActive, cancellationToken);
    }

    public void Add(Question question)
    {
        _context.Questions.Add(question);
    }
}
