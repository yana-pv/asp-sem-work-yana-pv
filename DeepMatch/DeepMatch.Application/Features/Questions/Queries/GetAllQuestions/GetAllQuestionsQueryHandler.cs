using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.Application.Features.Questions.Queries.GetAllQuestions;


public class GetAllQuestionsQueryHandler : IRequestHandler<GetAllQuestionsQuery, List<QuestionAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllQuestionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestionAdminDto>> Handle(GetAllQuestionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Questions
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
}
