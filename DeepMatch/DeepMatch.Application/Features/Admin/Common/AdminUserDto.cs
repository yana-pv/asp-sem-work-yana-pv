namespace DeepMatch.Application.Features.Admin.Common;

public record AdminUserDto(
    Guid Id,
    string Email,
    string UserName,
    string Role,
    int Age,
    int Rating,
    int ReportsCount,
    bool IsBlocked,
    string? BlockReason,
    DateTime RegisteredAt
);
