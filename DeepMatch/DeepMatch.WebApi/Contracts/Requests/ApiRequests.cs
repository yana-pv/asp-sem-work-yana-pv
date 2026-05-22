namespace DeepMatch.WebApi.Contracts.Requests;

public record RegisterRequest(string Email, string UserName, string Password, int Age);

public record LoginRequest(string Email, string Password);

public record CreateAnswerRequest(Guid QuestionId, string Text);

public record MarkNotificationReadRequest(Guid? NotificationId);

public record DevilsAdvocateRequest(string QuestionText, string UserAnswer);

public record SwipeAnswerRequest(Guid AnswerId, string Direction);

public record ReportUserRequest(Guid ReportedUserId, string Reason);

public record UpdateProfileRequest(string? Bio);

public record SendMessageRequest(Guid MatchId, string Content);

public record CreateQuestionRequest(string Text, string Category);

public record BlockUserRequest(string? Reason);
