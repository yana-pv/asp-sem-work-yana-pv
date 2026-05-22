using MediatR;
using DeepMatch.Application.Features.Profile.Common;

namespace DeepMatch.Application.Features.Profile.Queries.GetProfile;

public record GetProfileQuery : IRequest<ProfileDto?>;
