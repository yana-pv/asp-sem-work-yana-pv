namespace DeepMatch.Application.Features.Admin.Common;

public record AdminStatsDto(
    int UsersCount,
    int ActiveUsersCount,
    int BlockedUsersCount,
    int QuestionsCount,
    int AnswersCount,
    int MatchesCount,
    int ReportsCount
);
