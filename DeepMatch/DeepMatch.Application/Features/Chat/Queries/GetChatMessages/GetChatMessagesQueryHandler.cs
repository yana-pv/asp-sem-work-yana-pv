using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Entities;
using DeepMatch.Application.Features.Chat.Common;

namespace DeepMatch.Application.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, List<MessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChatMessagesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(Match), request.MatchId);
        }

        if (!match.InvolvesUser(_currentUser.UserId))
        {
            throw new ForbiddenException("Вы не участвуете в этом мэтче");
        }

        return await _context.Messages
            .Where(m => m.MatchId == request.MatchId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new MessageDto(m.Id, m.MatchId, m.Content, m.SenderUserId, m.Timestamp, m.IsIcebreaker))
            .ToListAsync(cancellationToken);
    }
}
