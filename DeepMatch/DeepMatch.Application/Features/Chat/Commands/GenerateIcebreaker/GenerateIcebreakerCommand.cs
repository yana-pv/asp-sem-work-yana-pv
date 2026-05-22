using MediatR;

namespace DeepMatch.Application.Features.Chat.Commands.GenerateIcebreaker;

public record GenerateIcebreakerCommand : IRequest<string>
{
    public Guid MatchId { get; init; }
}


