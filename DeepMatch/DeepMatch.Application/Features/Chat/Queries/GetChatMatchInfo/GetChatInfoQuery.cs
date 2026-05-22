using MediatR;
using DeepMatch.Application.Features.Chat.Common;

namespace DeepMatch.Application.Features.Chat.Queries.GetChatMatchInfo;

public record GetChatInfoQuery(Guid MatchId) : IRequest<ChatInfoDto>;
