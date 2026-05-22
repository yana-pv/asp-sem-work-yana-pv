namespace DeepMatch.Application.Features.Auth.Common;

public record AuthResponseDto(Guid UserId, string Email, string UserName, string Role);
