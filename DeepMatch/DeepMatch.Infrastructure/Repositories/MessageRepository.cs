using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Chat.Common;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<MessageDto>> GetMessagesByMatchAsync(Guid matchId, CancellationToken cancellationToken)
    {
        return _context.Messages
            .Where(m => m.MatchId == matchId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new MessageDto(m.Id, m.MatchId, m.Content, m.SenderUserId, m.Timestamp, m.IsIcebreaker))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Message>> GetUnreadByMatchForUserAsync(Guid matchId, Guid userId, CancellationToken cancellationToken)
    {
        return _context.Messages
            .Where(m =>
                m.MatchId == matchId &&
                m.SenderUserId != userId &&
                !m.IsRead)
            .ToListAsync(cancellationToken);
    }

    public void Add(Message message)
    {
        _context.Messages.Add(message);
    }
}
