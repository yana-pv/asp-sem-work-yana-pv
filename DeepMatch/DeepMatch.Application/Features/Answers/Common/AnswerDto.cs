namespace DeepMatch.Application.Features.Answers.Common;

public record AnswerDto(Guid Id, string Text, Guid QuestionId, DateTime CreatedAt);
