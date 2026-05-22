using MediatR;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<AdminUserDto>>
{
    private readonly IUserRepository _users;

    public GetUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<List<AdminUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _users.GetAdminUsersAsync(cancellationToken);
    }
}
