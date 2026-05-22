using DeepMatch.Domain.Constants;
using FluentValidation;

namespace DeepMatch.Application.Features.Answers.Commands.CreateAnswer;

public class CreateAnswerCommandValidator : AbstractValidator<CreateAnswerCommand>
{
    public CreateAnswerCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("ID вопроса обязателен");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст ответа обязателен")
            .MinimumLength(BusinessRules.Answers.MinTextLength)
            .WithMessage($"Ответ должен содержать не менее {BusinessRules.Answers.MinTextLength} символов")
            .MaximumLength(BusinessRules.Answers.MaxTextLength)
            .WithMessage($"Ответ должен содержать не более {BusinessRules.Answers.MaxTextLength} символов")
            .Must(NotContainDangerousHtml).WithMessage("Ответ содержит недопустимые HTML-конструкции");
    }

    private static bool NotContainDangerousHtml(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        var normalized = text.ToLowerInvariant();
        return !normalized.Contains("<script")
            && !normalized.Contains("javascript:")
            && !normalized.Contains("onerror=")
            && !normalized.Contains("onload=")
            && !normalized.Contains("<iframe")
            && !normalized.Contains("<object")
            && !normalized.Contains("<embed");
    }
}
