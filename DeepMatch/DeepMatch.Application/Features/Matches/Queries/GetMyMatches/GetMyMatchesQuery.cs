using MediatR;
using DeepMatch.Application.Features.Matches.Common;

namespace DeepMatch.Application.Features.Matches.Queries.GetMyMatches;

public record GetMyMatchesQuery : IRequest<List<MatchDto>>;
