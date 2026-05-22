using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Answers.Common;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class AnswerRepository : IAnswerRepository
{
    private readonly AppDbContext _context;

    public AnswerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Answer?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Answers
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public Task<Answer?> GetByUserAndQuestionAsync(Guid userId, Guid questionId, CancellationToken cancellationToken)
    {
        return _context.Answers
            .FirstOrDefaultAsync(a => a.UserId == userId && a.QuestionId == questionId, cancellationToken);
    }

    public Task<List<Guid>> GetAnswerIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Answers
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Answer>> GetUnprocessedBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        var answers = await _context.Answers.ToListAsync(cancellationToken);

        return answers
            .Where(a => a.Tags == null || a.Tags.Count == 0)
            .Take(batchSize)
            .ToList();
    }

    public Task<FeedCardDto?> GetRandomFeedCardAsync(
        Guid currentUserId,
        IReadOnlyCollection<Guid> swipedAnswerIds,
        CancellationToken cancellationToken)
    {
        return _context.Answers
            .Include(a => a.Question)
            .Where(a =>
                a.UserId != currentUserId &&
                !a.User.IsBlocked &&
                a.Question.IsActive &&
                !swipedAnswerIds.Contains(a.Id))
            .OrderBy(a => EF.Functions.Random())
            .Select(a => new FeedCardDto(
                a.Id,
                a.UserId,
                a.Text,
                a.Question.Text,
                a.Question.Category.ToString(),
                a.Tags,
                a.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<Answer>> GetAnswersByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Answers
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return _context.Answers.CountAsync(cancellationToken);
    }

    public void Add(Answer answer)
    {
        _context.Answers.Add(answer);
    }
}
