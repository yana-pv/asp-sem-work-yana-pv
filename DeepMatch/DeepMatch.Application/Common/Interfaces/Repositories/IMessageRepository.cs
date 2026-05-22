using DeepMatch.Application.Features.Chat.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IMessageRepository
{
    Task<List<MessageDto>> GetMessagesByMatchAsync(Guid matchId, CancellationToken cancellationToken);
    Task<List<Message>> GetUnreadByMatchForUserAsync(Guid matchId, Guid userId, CancellationToken cancellationToken);
    void Add(Message message);
}
