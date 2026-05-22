using MediatR;

namespace DeepMatch.Application.Features.Questions.Commands.CreateQuestion;

public record CreateQuestionCommand(string Text, string Category) : IRequest<Guid>;



