namespace DeepMatch.Application.Features.Reports.Common;

public record ReportDto(
    Guid Id,
    string ReporterName,
    string ReportedName,
    Guid ReportedUserId,
    string Reason,
    int ReportsCount,
    bool IsBlocked,
    DateTime CreatedAt
);
