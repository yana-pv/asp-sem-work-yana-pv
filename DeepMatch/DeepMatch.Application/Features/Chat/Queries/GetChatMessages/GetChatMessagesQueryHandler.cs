using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Chat.Common;

namespace DeepMatch.Application.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, List<MessageDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;
    private readonly ICurrentUserService _currentUser;

    public GetChatMessagesQueryHandler(
        IMatchRepository matches,
        IMessageRepository messages,
        ICurrentUserService currentUser)
    {
        _matches = matches;
        _messages = messages;
        _currentUser = currentUser;
    }

    public async Task<List<MessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(Match), request.MatchId);
        }

        if (!match.InvolvesUser(_currentUser.UserId))
        {
            throw new ForbiddenException("Вы не участвуете в этом мэтче");
        }

        return await _messages.GetMessagesByMatchAsync(request.MatchId, cancellationToken);
    }
}
