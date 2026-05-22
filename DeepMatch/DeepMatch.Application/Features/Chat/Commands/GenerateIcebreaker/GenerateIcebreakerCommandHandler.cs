using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Chat.Commands.GenerateIcebreaker;

public class GenerateIcebreakerCommandHandler : IRequestHandler<GenerateIcebreakerCommand, string>
{
    private readonly IMatchRepository _matches;
    private readonly IAnswerRepository _answers;
    private readonly IMessageRepository _messages;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAiService _aiService;

    public GenerateIcebreakerCommandHandler(
        IMatchRepository matches,
        IAnswerRepository answers,
        IMessageRepository messages,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAiService aiService)
    {
        _matches = matches;
        _answers = answers;
        _messages = messages;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _aiService = aiService;
    }

    public async Task<string> Handle(GenerateIcebreakerCommand request, CancellationToken cancellationToken)
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

        var otherUserId = match.GetOtherUserId(currentUserId);

        var myAnswers = await _answers.GetAnswersByUserIdAsync(currentUserId, cancellationToken);
        var myTags = myAnswers
            .Where(a => a.Tags != null && a.Tags.Any())
            .SelectMany(a => a.Tags)
            .Distinct()
            .ToList();

        var otherAnswers = await _answers.GetAnswersByUserIdAsync(otherUserId, cancellationToken);
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

        _messages.Add(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return icebreaker;
    }
}
