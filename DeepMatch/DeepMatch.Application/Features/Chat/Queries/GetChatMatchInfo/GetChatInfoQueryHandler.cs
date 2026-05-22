using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Chat.Common;
using DeepMatch.Application.Features.Chat.Mappers;

namespace DeepMatch.Application.Features.Chat.Queries.GetChatMatchInfo;

public class GetChatInfoQueryHandler : IRequestHandler<GetChatInfoQuery, ChatInfoDto>
{
    private readonly IMatchRepository _matches;
    private readonly ICurrentUserService _currentUser;

    public GetChatInfoQueryHandler(IMatchRepository matches, ICurrentUserService currentUser)
    {
        _matches = matches;
        _currentUser = currentUser;
    }

    public async Task<ChatInfoDto> Handle(GetChatInfoQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var match = await _matches.GetByIdWithChatInfoAsync(request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(Match), request.MatchId);
        }

        if (!match.InvolvesUser(currentUserId))
        {
            throw new ForbiddenException("Вы не участвуете в этом мэтче");
        }

        return ChatMapper.ToChatInfoDto(match, currentUserId);
    }
}
