using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Answers.Common;

namespace DeepMatch.Application.Features.Answers.Queries.GetFeed;

public class GetFeedQueryHandler : IRequestHandler<GetFeedQuery, FeedCardDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFeedQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FeedCardDto?> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var swipedAnswerIds = await _context.Swipes
            .Where(s => s.SwiperUserId == currentUserId)
            .Select(s => s.TargetAnswerId)
            .ToListAsync(cancellationToken);

        var answer = await _context.Answers
            .Include(a => a.Question)
            .Where(a =>
                a.UserId != currentUserId &&
                !a.User.IsBlocked &&
                a.Question.IsActive &&
                !swipedAnswerIds.Contains(a.Id))
            .OrderBy(a => EF.Functions.Random())
            .Select(a => new FeedCardDto(
                a.Id,
                a.UserId,
                a.Text,
                a.Question.Text,
                a.Question.Category.ToString(),
                a.Tags,
                a.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return answer;
    }
}
