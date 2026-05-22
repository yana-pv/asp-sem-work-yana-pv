using MediatR;
using DeepMatch.Application.Features.Answers.Common;

namespace DeepMatch.Application.Features.Answers.Queries.GetFeed;

public record GetFeedQuery : IRequest<FeedCardDto?>;
