using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Matches.Queries.GetMyMatches;
using DeepMatch.Application.Features.Matches.Common;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Мэтчи текущего пользователя.
/// </summary>
[Authorize]
public class MatchesController : ApiControllerBase
{
    /// <summary>
    /// Получить список моих мэтчей.
    /// </summary>
    /// <returns>Список мэтчей, отсортированный по последней активности.</returns>
    /// <response code="200">Список мэтчей получен.</response>
    [HttpGet]
    public async Task<ActionResult<List<MatchDto>>> GetMatches()
    {
        return Ok(await Mediator.Send(new GetMyMatchesQuery()));
    }
}
