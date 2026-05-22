using MediatR;

namespace DeepMatch.Application.Features.AiAssistant.Queries.GetDevilsAdvocate;

public record GetDevilsAdvocateQuery : IRequest<string>
{
    public string QuestionText { get; init; } = string.Empty;
    public string UserAnswer { get; init; } = string.Empty;
}
