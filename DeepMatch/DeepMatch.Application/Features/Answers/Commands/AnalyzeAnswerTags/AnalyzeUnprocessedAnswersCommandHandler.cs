using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;

namespace DeepMatch.Application.Features.Answers.Commands.AnalyzeAnswerTags;

public class AnalyzeUnprocessedAnswersCommandHandler : IRequestHandler<AnalyzeUnprocessedAnswersCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IAiService _aiService;

    public AnalyzeUnprocessedAnswersCommandHandler(IApplicationDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task Handle(AnalyzeUnprocessedAnswersCommand request, CancellationToken cancellationToken)
    {
        var allAnswers = await _context.Answers.ToListAsync(cancellationToken);
        var unprocessed = allAnswers
            .Where(a => a.Tags == null || a.Tags.Count == 0)
            .Take(BusinessRules.Answers.AnalyzeTagsBatchSize)
            .ToList();

        foreach (var answer in unprocessed)
        {
            try
            {
                var tags = await _aiService.AnalyzeAnswerTagsAsync(answer.Text);
                answer.Tags = tags;
            }
            catch
            {
                answer.Tags = new List<string>();
            }
        }

        if (unprocessed.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
