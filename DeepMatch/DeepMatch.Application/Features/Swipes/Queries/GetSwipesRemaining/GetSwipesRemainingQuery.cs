using MediatR;
using DeepMatch.Application.Features.Swipes.Common;

namespace DeepMatch.Application.Features.Swipes.Queries.GetSwipesRemaining;

public record GetSwipesRemainingQuery : IRequest<SwipesRemainingDto>;
