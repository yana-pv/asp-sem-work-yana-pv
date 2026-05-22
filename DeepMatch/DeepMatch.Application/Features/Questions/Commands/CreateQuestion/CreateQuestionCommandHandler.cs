using FluentValidation.Results;
using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;
using AppValidationException = DeepMatch.Application.Common.Exceptions.ValidationException;

namespace DeepMatch.Application.Features.Questions.Commands.CreateQuestion;



public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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

        _context.Questions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}
