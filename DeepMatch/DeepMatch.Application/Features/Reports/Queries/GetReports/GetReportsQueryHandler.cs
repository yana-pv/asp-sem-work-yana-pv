using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Reports.Common;

namespace DeepMatch.Application.Features.Reports.Queries.GetReports;


public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, List<ReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Reports
            .Include(r => r.ReporterUser)
            .Include(r => r.ReportedUser)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportDto(
                r.Id,
                r.ReporterUser.UserName,
                r.ReportedUser.UserName,
                r.ReportedUserId,
                r.Reason,
                r.ReportedUser.ReportsCount,
                r.ReportedUser.IsBlocked,
                r.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
