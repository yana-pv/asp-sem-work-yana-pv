namespace DeepMatch.Application.Features.Matches.Common;

public record MatchDto(
    Guid MatchId,
    string MatchedUserName,
    Guid MatchedUserId,
    DateTime CreatedAt,
    bool HasUnreadMessages);
