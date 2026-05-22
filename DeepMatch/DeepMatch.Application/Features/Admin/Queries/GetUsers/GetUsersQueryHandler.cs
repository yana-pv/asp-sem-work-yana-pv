using MediatR;
using Microsoft.EntityFrameworkCore;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<AdminUserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .Where(u => u.Role != UserRoles.System)
            .OrderByDescending(u => u.RegisteredAt)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email,
                u.UserName,
                u.Role,
                u.Age,
                u.Rating.Value,
                u.ReportsCount,
                u.IsBlocked,
                u.BlockReason,
                u.RegisteredAt))
            .ToListAsync(cancellationToken);
    }
}
