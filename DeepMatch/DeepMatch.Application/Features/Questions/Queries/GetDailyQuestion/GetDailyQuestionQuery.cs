using MediatR;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.Application.Features.Questions.Queries.GetDailyQuestion;

public record GetDailyQuestionQuery : IRequest<DailyQuestionDto?>;
