using DeepMatch.Application.Features.Admin.Commands.DeleteUser;
using DeepMatch.Application.Features.Admin.Queries.GetUsers;
using DeepMatch.Domain.Constants;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.WebApi.Controllers.Admin;

/// <summary>
/// Управление пользователями для администратора.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminUsersController : ApiControllerBase
{
    /// <summary>
    /// Получить список пользователей.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        return await Mediator.Send(new GetUsersQuery());
    }

    /// <summary>
    /// Заблокировать пользователя.
    /// </summary>
    [HttpPost("{userId:guid}/block")]
    public async Task<ActionResult> BlockUser(Guid userId, BlockUserRequest request)
    {
        await Mediator.Send(new DeleteUserCommand(userId, request.Reason));
        return Ok(new { message = "Пользователь заблокирован" });
    }
}
