using MediatR;
using DeepMatch.Application.Features.Swipes.Common;

namespace DeepMatch.Application.Features.Swipes.Commands.SwipeAnswer;

public record SwipeAnswerCommand : IRequest<SwipeResultDto>
{
    public Guid AnswerId { get; init; }
    public string Direction { get; init; } = string.Empty;
}
