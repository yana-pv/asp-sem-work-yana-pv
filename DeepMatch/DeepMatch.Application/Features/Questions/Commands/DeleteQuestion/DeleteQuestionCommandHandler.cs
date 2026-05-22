using MediatR;
using DeepMatch.Application.Common.Exceptions;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Features.Questions.Commands.DeleteQuestion;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IQuestionRepository _questions;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteQuestionCommandHandler(IQuestionRepository questions, IUnitOfWork unitOfWork)
    {
        _questions = questions;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _questions.GetByIdAsync(request.QuestionId, cancellationToken);

        if (question == null)
        {
            throw new NotFoundException(nameof(Question), request.QuestionId);
        }

        question.IsActive = false;
        question.DateOfDay = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
