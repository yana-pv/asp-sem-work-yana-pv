using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.Application.Features.Questions.Queries.GetDailyQuestion;

public class GetDailyQuestionQueryHandler : IRequestHandler<GetDailyQuestionQuery, DailyQuestionDto?>
{
    private readonly IQuestionRepository _questions;

    public GetDailyQuestionQueryHandler(IQuestionRepository questions)
    {
        _questions = questions;
    }

    public async Task<DailyQuestionDto?> Handle(GetDailyQuestionQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _questions.GetDailyQuestionAsync(today, cancellationToken);
    }
}
