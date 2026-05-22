using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Chat.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand>
{
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkMessagesAsReadCommandHandler(
        IMatchRepository matches,
        IMessageRepository messages,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _matches = matches;
        _messages = messages;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(Match), request.MatchId);
        }

        if (!match.InvolvesUser(currentUserId))
        {
            throw new ForbiddenException("Вы не участвуете в этом мэтче");
        }

        var unreadMessages = await _messages.GetUnreadByMatchForUserAsync(request.MatchId, currentUserId, cancellationToken);
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
