using MediatR;
using DeepMatch.Application.Features.Profile.Common;

namespace DeepMatch.Application.Features.Profile.Queries.GetPublicProfile;

public record GetPublicProfileQuery(Guid UserId) : IRequest<PublicProfileDto>;


