using MediatR;
using DeepMatch.Application.Common.Interfaces;

namespace DeepMatch.Application.Features.AiAssistant.Queries.GetDevilsAdvocate;

public class GetDevilsAdvocateQueryHandler : IRequestHandler<GetDevilsAdvocateQuery, string>
{
    private readonly IAiService _aiService;

    public GetDevilsAdvocateQueryHandler(IAiService aiService)
    {
        _aiService = aiService;
    }

    public async Task<string> Handle(GetDevilsAdvocateQuery request, CancellationToken cancellationToken)
    {
        return await _aiService.GetDevilsAdvocateAsync(request.QuestionText, request.UserAnswer);
    }
}
