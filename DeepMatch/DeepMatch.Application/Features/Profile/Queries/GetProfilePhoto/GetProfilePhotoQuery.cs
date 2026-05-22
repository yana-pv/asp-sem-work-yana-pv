using MediatR;
using DeepMatch.Application.Features.Profile.Queries.GetAvatar;

namespace DeepMatch.Application.Features.Profile.Queries.GetProfilePhoto;

public record GetProfilePhotoQuery(Guid PhotoId) : IRequest<AvatarResult?>;