using MediatR;
using DeepMatch.Application.Features.Reports.Common;

namespace DeepMatch.Application.Features.Reports.Queries.GetReports;

public record GetReportsQuery : IRequest<List<ReportDto>>;


