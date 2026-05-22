using DeepMatch.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeepMatch.WebApi.Controllers.Admin;

/// <summary>
/// Тестовый административный endpoint для проверки role-based авторизации.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminTestController : ApiControllerBase
{
    /// <summary>
    /// Проверить доступ пользователя с ролью Admin.
    /// </summary>
    /// <returns>Сообщение об успешном доступе.</returns>
    /// <response code="200">Пользователь имеет роль Admin.</response>
    /// <response code="403">У пользователя нет роли Admin.</response>
    [HttpGet("test")]
    public ActionResult Test()
    {
        return Ok(new { message = "Вы админ, доступ разрешён" });
    }
}
