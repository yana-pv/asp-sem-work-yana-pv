using DeepMatch.Application.Features.Reports.Common;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<bool> ExistsByReporterAndReportedAsync(Guid reporterId, Guid reportedId, CancellationToken cancellationToken);
    Task<List<ReportDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    void Add(Report report);
}
