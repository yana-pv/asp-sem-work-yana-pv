using DeepMatch.Application.Features.Reports.Commands.ReportUser;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Жалобы пользователей на неподходящий контент или поведение.
/// </summary>
[Authorize]
public class ReportsController : ApiControllerBase
{
    /// <summary>
    /// Отправить жалобу на пользователя.
    /// </summary>
    /// <param name="request">ID пользователя и причина жалобы.</param>
    /// <response code="200">Жалоба отправлена.</response>
    /// <response code="400">Некорректная причина или повторная жалоба.</response>
    /// <response code="403">Жалоба на этого пользователя запрещена.</response>
    [HttpPost]
    public async Task<ActionResult> ReportUser(ReportUserRequest request)
    {
        await Mediator.Send(new ReportUserCommand(request.ReportedUserId, request.Reason));
        return Ok(new { message = "Жалоба отправлена" });
    }
}
