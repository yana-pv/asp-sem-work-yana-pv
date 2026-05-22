using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Answers.Common;

namespace DeepMatch.Application.Features.Answers.Queries.GetFeed;

public class GetFeedQueryHandler : IRequestHandler<GetFeedQuery, FeedCardDto?>
{
    private readonly IAnswerRepository _answers;
    private readonly ISwipeRepository _swipes;
    private readonly ICurrentUserService _currentUser;

    public GetFeedQueryHandler(
        IAnswerRepository answers,
        ISwipeRepository swipes,
        ICurrentUserService currentUser)
    {
        _answers = answers;
        _swipes = swipes;
        _currentUser = currentUser;
    }

    public async Task<FeedCardDto?> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        var swipedAnswerIds = await _swipes.GetTargetAnswerIdsBySwiperAsync(currentUserId, cancellationToken);

        return await _answers.GetRandomFeedCardAsync(currentUserId, swipedAnswerIds, cancellationToken);
    }
}
