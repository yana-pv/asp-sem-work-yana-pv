using FluentValidation.Results;
using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;
using AppValidationException = DeepMatch.Application.Common.Exceptions.ValidationException;

namespace DeepMatch.Application.Features.Questions.Commands.CreateQuestion;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Guid>
{
    private readonly IQuestionRepository _questions;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuestionCommandHandler(IQuestionRepository questions, IUnitOfWork unitOfWork)
    {
        _questions = questions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<QuestionCategory>(request.Category, true, out var category))
        {
            throw new AppValidationException(new List<ValidationFailure>
            {
                new("Category", "Неизвестная категория вопроса")
            });
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = request.Text.Trim(),
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _questions.Add(question);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}
