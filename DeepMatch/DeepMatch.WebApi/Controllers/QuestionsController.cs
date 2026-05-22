using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Questions.Queries.GetDailyQuestion;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Вопросы дня для участия в DeepMatch.
/// </summary>
[Authorize]
public class QuestionsController : ApiControllerBase
{
    /// <summary>
    /// Получить текущий вопрос дня.
    /// </summary>
    /// <returns>Вопрос дня или 404, если он еще не назначен.</returns>
    /// <response code="200">Вопрос дня найден.</response>
    /// <response code="404">Вопрос дня еще не назначен.</response>
    [HttpGet("daily")]
    public async Task<ActionResult<DailyQuestionDto>> GetDailyQuestion()
    {
        var question = await Mediator.Send(new GetDailyQuestionQuery());

        if (question == null)
        {
            return NotFound(new { message = "Вопрос дня ещё не назначен" });
        }

        return Ok(question);
    }
}
