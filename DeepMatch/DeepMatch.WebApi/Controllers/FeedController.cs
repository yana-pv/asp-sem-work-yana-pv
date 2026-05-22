using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Answers.Queries.GetFeed;
using DeepMatch.Application.Features.Answers.Common;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Лента анонимных ответов для свайпов.
/// </summary>
[Authorize]
public class FeedController : ApiControllerBase
{
    /// <summary>
    /// Получить случайную карточку с ответом другого пользователя.
    /// </summary>
    /// <returns>Карточка ответа или 404, если новых ответов нет.</returns>
    /// <response code="200">Карточка найдена.</response>
    /// <response code="404">Нет новых ответов для просмотра.</response>
    [HttpGet]
    public async Task<ActionResult<FeedCardDto>> GetFeed()
    {
        var card = await Mediator.Send(new GetFeedQuery());

        if (card == null)
            return NotFound(new { message = "Нет новых ответов для просмотра" });

        return Ok(card);
    }
}
