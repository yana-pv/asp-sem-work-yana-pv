using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;

namespace DeepMatch.Application.Features.Answers.Commands.AnalyzeAnswerTags;

public class AnalyzeUnprocessedAnswersCommandHandler : IRequestHandler<AnalyzeUnprocessedAnswersCommand>
{
    private readonly IAnswerRepository _answers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiService _aiService;

    public AnalyzeUnprocessedAnswersCommandHandler(
        IAnswerRepository answers,
        IUnitOfWork unitOfWork,
        IAiService aiService)
    {
        _answers = answers;
        _unitOfWork = unitOfWork;
        _aiService = aiService;
    }

    public async Task Handle(AnalyzeUnprocessedAnswersCommand request, CancellationToken cancellationToken)
    {
        var unprocessed = await _answers.GetUnprocessedBatchAsync(BusinessRules.Answers.AnalyzeTagsBatchSize, cancellationToken);

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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
