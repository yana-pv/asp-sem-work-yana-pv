namespace DeepMatch.Application.Features.Answers.Common;

public record FeedCardDto(
    Guid AnswerId,
    Guid AuthorUserId,
    string AnswerText,
    string QuestionText,
    string Category,
    List<string> Tags,
    DateTime CreatedAt
);
