using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Matches.Common;

namespace DeepMatch.Application.Features.Matches.Queries.GetMyMatches;

public class GetMyMatchesQueryHandler : IRequestHandler<GetMyMatchesQuery, List<MatchDto>>
{
    private readonly IMatchRepository _matches;
    private readonly ICurrentUserService _currentUser;

    public GetMyMatchesQueryHandler(IMatchRepository matches, ICurrentUserService currentUser)
    {
        _matches = matches;
        _currentUser = currentUser;
    }

    public async Task<List<MatchDto>> Handle(GetMyMatchesQuery request, CancellationToken cancellationToken)
    {
        return await _matches.GetMatchesForUserAsync(_currentUser.UserId, cancellationToken);
    }
}
