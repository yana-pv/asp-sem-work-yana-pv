using MediatR;

using DeepMatch.Application.Features.Answers.Common;

namespace DeepMatch.Application.Features.Answers.Commands.CreateAnswer;

public record CreateAnswerCommand : IRequest<AnswerDto>
{
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
}
