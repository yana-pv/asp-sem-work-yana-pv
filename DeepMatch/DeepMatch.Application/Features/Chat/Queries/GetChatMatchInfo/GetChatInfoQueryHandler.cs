using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Chat.Common;

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

        var isCurrentFirstUser = match.User1Id == currentUserId;

        var currentUserName = isCurrentFirstUser ? match.User1.UserName : match.User2.UserName;
        var matchedUserName = isCurrentFirstUser ? match.User2.UserName : match.User1.UserName;

        var currentAnswer = isCurrentFirstUser ? match.CatalystAnswer1 : match.CatalystAnswer2;
        var matchedAnswer = isCurrentFirstUser ? match.CatalystAnswer2 : match.CatalystAnswer1;

        return new ChatInfoDto(
            match.Id,
            currentUserName,
            matchedUserName,
            currentAnswer.Question.Text,
            currentAnswer.Text,
            matchedAnswer.Question.Text,
            matchedAnswer.Text
        );
    }
}
