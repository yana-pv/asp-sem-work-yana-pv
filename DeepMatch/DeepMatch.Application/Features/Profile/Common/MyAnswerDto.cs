namespace DeepMatch.Application.Features.Profile.Common;

public record MyAnswerDto(Guid Id, string Text, string QuestionText, DateTime CreatedAt, List<string> Tags);
