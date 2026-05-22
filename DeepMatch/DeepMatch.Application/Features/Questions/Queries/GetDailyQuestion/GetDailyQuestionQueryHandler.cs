using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.Application.Features.Questions.Queries.GetDailyQuestion;

public class GetDailyQuestionQueryHandler : IRequestHandler<GetDailyQuestionQuery, DailyQuestionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDailyQuestionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyQuestionDto?> Handle(GetDailyQuestionQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.Questions
            .Where(q => q.DateOfDay == today && q.IsActive)
            .Select(q => new DailyQuestionDto(q.Id, q.Text, q.Category.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
