using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Admin.Queries.GetStats;
using DeepMatch.Domain.Constants;
using DeepMatch.Application.Features.Admin.Common;

namespace DeepMatch.WebApi.Controllers.Admin;

/// <summary>
/// Административная статистика DeepMatch.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminStatsController : ApiControllerBase
{
    /// <summary>
    /// Получить основные показатели приложения.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        return await Mediator.Send(new GetStatsQuery());
    }
}
