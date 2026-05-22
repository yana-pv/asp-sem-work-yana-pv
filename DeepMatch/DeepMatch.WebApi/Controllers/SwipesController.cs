using DeepMatch.Application.Features.Swipes.Commands.SwipeAnswer;
using DeepMatch.Application.Features.Swipes.Queries.GetSwipesRemaining;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Swipes.Common;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Свайпы ответов: лайки, пропуски и дневной лимит.
/// </summary>
[Authorize]
public class SwipesController : ApiControllerBase
{
    /// <summary>
    /// Свайпнуть ответ другого пользователя.
    /// </summary>
    /// <param name="request">ID ответа и направление: like или pass.</param>
    /// <returns>Результат свайпа и информация о мэтче, если он создан.</returns>
    /// <response code="200">Свайп выполнен.</response>
    /// <response code="400">Некорректное направление или повторный свайп.</response>
    /// <response code="403">Попытка свайпнуть собственный ответ.</response>
    /// <response code="429">Дневной лимит свайпов исчерпан.</response>
    [HttpPost]
    public async Task<ActionResult<SwipeResultDto>> Swipe(SwipeAnswerRequest request)
    {
        var command = new SwipeAnswerCommand
        {
            AnswerId = request.AnswerId,
            Direction = request.Direction
        };

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Получить количество оставшихся свайпов на сегодня.
    /// </summary>
    /// <returns>Лимит, использованное и оставшееся количество свайпов.</returns>
    /// <response code="200">Информация о лимите получена.</response>
    [HttpGet("remaining")]
    public async Task<ActionResult<SwipesRemainingDto>> GetRemaining()
    {
        return await Mediator.Send(new GetSwipesRemainingQuery());
    }
}
