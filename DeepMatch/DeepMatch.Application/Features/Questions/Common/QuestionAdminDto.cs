namespace DeepMatch.Application.Features.Questions.Common;

public record QuestionAdminDto(
    Guid Id,
    string Text,
    string Category,
    DateOnly? DateOfDay,
    bool IsActive,
    int AnswersCount,
    DateTime CreatedAt
);
