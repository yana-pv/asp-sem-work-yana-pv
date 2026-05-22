using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Reports.Common;

namespace DeepMatch.Application.Features.Reports.Queries.GetReports;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, List<ReportDto>>
{
    private readonly IReportRepository _reports;

    public GetReportsQueryHandler(IReportRepository reports)
    {
        _reports = reports;
    }

    public async Task<List<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        return await _reports.GetAllAsync(cancellationToken);
    }
}
