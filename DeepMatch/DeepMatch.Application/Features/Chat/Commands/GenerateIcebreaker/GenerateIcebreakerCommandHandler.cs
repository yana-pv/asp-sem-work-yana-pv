using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Chat.Commands.GenerateIcebreaker;

public class GenerateIcebreakerCommandHandler : IRequestHandler<GenerateIcebreakerCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiService _aiService;

    public GenerateIcebreakerCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAiService aiService)
    {
        _context = context;
        _currentUser = currentUser;
        _aiService = aiService;
    }

    public async Task<string> Handle(GenerateIcebreakerCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(Match), request.MatchId);
        }

        if (!match.InvolvesUser(currentUserId))
        {
            throw new ForbiddenException("Вы не участвуете в этом мэтче");
        }

        var otherUserId = match.GetOtherUserId(currentUserId);

        var myAnswers = await _context.Answers
            .Where(a => a.UserId == currentUserId)
            .ToListAsync(cancellationToken);
        var myTags = myAnswers
            .Where(a => a.Tags != null && a.Tags.Any())
            .SelectMany(a => a.Tags)
            .Distinct()
            .ToList();

        var otherAnswers = await _context.Answers
            .Where(a => a.UserId == otherUserId)
            .ToListAsync(cancellationToken);
        var otherTags = otherAnswers
            .Where(a => a.Tags != null && a.Tags.Any())
            .SelectMany(a => a.Tags)
            .Distinct()
            .ToList();

        if (!myTags.Any()) myTags = new List<string> { "человек" };
        if (!otherTags.Any()) otherTags = new List<string> { "человек" };

        var icebreaker = await _aiService.GenerateIcebreakerAsync(myTags, otherTags);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            MatchId = request.MatchId,
            SenderUserId = SystemUsers.DeepMatchUserId,
            Content = icebreaker,
            Timestamp = DateTime.UtcNow,
            IsIcebreaker = true,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return icebreaker;
    }
}
