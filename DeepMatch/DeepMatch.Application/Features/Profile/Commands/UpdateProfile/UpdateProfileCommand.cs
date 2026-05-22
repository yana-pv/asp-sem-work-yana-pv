using MediatR;

namespace DeepMatch.Application.Features.Profile.Commands.UpdateProfile;

public record UpdateProfileCommand(string? Bio) : IRequest;

