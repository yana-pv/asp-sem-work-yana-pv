using MediatR;

namespace DeepMatch.Application.Features.Chat.Commands.MarkMessagesAsRead;

public record MarkMessagesAsReadCommand(Guid MatchId) : IRequest;


