using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Reports.Queries.GetReports;
using DeepMatch.Domain.Constants;
using DeepMatch.Application.Features.Reports.Common;

namespace DeepMatch.WebApi.Controllers.Admin;

/// <summary>
/// Просмотр жалоб пользователей для администратора.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminReportsController : ApiControllerBase
{
    /// <summary>
    /// Получить все жалобы.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ReportDto>>> GetReports()
    {
        return await Mediator.Send(new GetReportsQuery());
    }
}
