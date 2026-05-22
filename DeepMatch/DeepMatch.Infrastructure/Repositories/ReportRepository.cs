using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Reports.Common;
using DeepMatch.Domain.Entities;
using DeepMatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeepMatch.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsByReporterAndReportedAsync(Guid reporterId, Guid reportedId, CancellationToken cancellationToken)
    {
        return _context.Reports.AnyAsync(
            r => r.ReporterUserId == reporterId && r.ReportedUserId == reportedId,
            cancellationToken);
    }

    public Task<List<ReportDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _context.Reports
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

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return _context.Reports.CountAsync(cancellationToken);
    }

    public void Add(Report report)
    {
        _context.Reports.Add(report);
    }
}
