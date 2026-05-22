using MediatR;

namespace DeepMatch.Application.Features.Profile.Queries.GetAvatar;

public record GetAvatarQuery(Guid UserId) : IRequest<AvatarResult?>;

