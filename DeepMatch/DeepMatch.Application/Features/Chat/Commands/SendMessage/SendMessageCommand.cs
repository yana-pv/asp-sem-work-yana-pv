using MediatR;
using DeepMatch.Application.Features.Chat.Common;

namespace DeepMatch.Application.Features.Chat.Commands.SendMessage;

public record SendMessageCommand : IRequest<MessageDto>
{
    public Guid MatchId { get; init; }
    public string Content { get; init; } = string.Empty;
}
