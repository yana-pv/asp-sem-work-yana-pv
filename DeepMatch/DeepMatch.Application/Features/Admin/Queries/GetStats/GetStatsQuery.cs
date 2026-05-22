using MediatR;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.Application.Features.Admin.Queries.GetStats;

public record GetStatsQuery : IRequest<AdminStatsDto>;
