using MediatR;

namespace DeepMatch.Application.Features.Questions.Commands.DeleteQuestion;

public record DeleteQuestionCommand(Guid QuestionId) : IRequest;