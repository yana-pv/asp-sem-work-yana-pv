using DeepMatch.Domain.Constants;
using FluentValidation;

namespace DeepMatch.Application.Features.Questions.Commands.CreateQuestion;

public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст вопроса обязателен")
            .MinimumLength(BusinessRules.Questions.MinTextLength)
            .WithMessage($"Вопрос должен быть не короче {BusinessRules.Questions.MinTextLength} символов")
            .MaximumLength(BusinessRules.Questions.MaxTextLength)
            .WithMessage($"Вопрос должен быть не длиннее {BusinessRules.Questions.MaxTextLength} символов");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Категория обязательна");
    }
}
