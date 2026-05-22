namespace DeepMatch.Application.Features.Chat.Common;

public record ChatInfoDto(
    Guid MatchId,
    string CurrentUserName,
    string MatchedUserName,
    string CurrentQuestionText,
    string CurrentAnswerText,
    string MatchedQuestionText,
    string MatchedAnswerText);
