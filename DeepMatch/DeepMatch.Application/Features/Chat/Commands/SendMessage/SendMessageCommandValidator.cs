using DeepMatch.Domain.Constants;
using FluentValidation;

namespace DeepMatch.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("ID мэтча обязателен");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Сообщение не может быть пустым")
            .MaximumLength(BusinessRules.Messages.MaxContentLength)
            .WithMessage($"Сообщение должно содержать не более {BusinessRules.Messages.MaxContentLength} символов")
            .Must(NotContainDangerousHtml).WithMessage("Сообщение содержит недопустимые HTML-конструкции");
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
