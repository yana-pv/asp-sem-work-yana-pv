using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Chat.Common;

namespace DeepMatch.Application.Features.Chat.Queries.GetChatMatchInfo;

public class GetChatInfoQueryHandler : IRequestHandler<GetChatInfoQuery, ChatInfoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChatInfoQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ChatInfoDto> Handle(GetChatInfoQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var match = await _context.Matches
            .Include(m => m.User1)
            .Include(m => m.User2)
            .Include(m => m.CatalystAnswer1)
            .ThenInclude(a => a.Question)
            .Include(m => m.CatalystAnswer2)
            .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

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
