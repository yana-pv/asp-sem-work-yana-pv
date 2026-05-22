using MediatR;
using DeepMatch.Application.Features.Chat.Common;

namespace DeepMatch.Application.Features.Chat.Queries.GetChatMessages;

public record GetChatMessagesQuery(Guid MatchId) : IRequest<List<MessageDto>>;
