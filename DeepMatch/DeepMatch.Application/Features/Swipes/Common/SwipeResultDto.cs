namespace DeepMatch.Application.Features.Swipes.Common;

public record SwipeResultDto(bool IsMatch, Guid? MatchId = null, string? MatchedUserName = null);
