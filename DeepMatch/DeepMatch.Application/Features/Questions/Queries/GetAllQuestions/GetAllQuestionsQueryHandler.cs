using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.Application.Features.Questions.Queries.GetAllQuestions;

public class GetAllQuestionsQueryHandler : IRequestHandler<GetAllQuestionsQuery, List<QuestionAdminDto>>
{
    private readonly IQuestionRepository _questions;

    public GetAllQuestionsQueryHandler(IQuestionRepository questions)
    {
        _questions = questions;
    }

    public async Task<List<QuestionAdminDto>> Handle(GetAllQuestionsQuery request, CancellationToken cancellationToken)
    {
        return await _questions.GetAllAdminAsync(cancellationToken);
    }
}
