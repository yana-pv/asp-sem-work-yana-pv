using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Matches.Common;

namespace DeepMatch.Application.Features.Matches.Queries.GetMyMatches;

public class GetMyMatchesQueryHandler : IRequestHandler<GetMyMatchesQuery, List<MatchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyMatchesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MatchDto>> Handle(GetMyMatchesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        return await _context.Matches
            .Where(m => m.User1Id == currentUserId || m.User2Id == currentUserId)
            .Select(m => new
            {
                Match = m,
                MatchedUserName = m.User1Id == currentUserId ? m.User2.UserName : m.User1.UserName,
                MatchedUserId = m.User1Id == currentUserId ? m.User2Id : m.User1Id,
                LastActivityAt = _context.Messages
                    .Where(message => message.MatchId == m.Id)
                    .Select(message => (DateTime?)message.Timestamp)
                    .Max() ?? m.CreatedAt,
                HasUnreadMessages = _context.Messages
                    .Any(message =>
                        message.MatchId == m.Id &&
                        message.SenderUserId != currentUserId &&
                        !message.IsRead)
            })
            .OrderByDescending(m => m.LastActivityAt)
            .Select(m => new MatchDto(
                m.Match.Id,
                m.MatchedUserName,
                m.MatchedUserId,
                m.LastActivityAt,
                m.HasUnreadMessages
            ))
            .ToListAsync(cancellationToken);
    }
}
