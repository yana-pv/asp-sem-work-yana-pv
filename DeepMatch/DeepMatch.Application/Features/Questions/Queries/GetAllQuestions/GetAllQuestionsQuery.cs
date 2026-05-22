using MediatR;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.Application.Features.Questions.Queries.GetAllQuestions;

public record GetAllQuestionsQuery : IRequest<List<QuestionAdminDto>>;


