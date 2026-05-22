using MediatR;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<AdminUserDto>>;


