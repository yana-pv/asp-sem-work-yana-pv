using DeepMatch.Application.Features.AiAssistant.Queries.GetDevilsAdvocate;
using DeepMatch.WebApi.Contracts.Requests;
using DeepMatch.WebApi.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// AI-помощник для анализа ответов и поддержки диалога.
/// </summary>
[Authorize]
public class AiAssistantController : ApiControllerBase
{
    /// <summary>
    /// Получить взгляд в стиле "адвоката дьявола".
    /// </summary>
    /// <param name="request">Текст вопроса и ответ пользователя.</param>
    /// <returns>Позиция адвоката дьявола по теме ответа.</returns>
    /// <response code="200">Ответ адвоката дьявола сгенерирован.</response>
    [HttpPost("devils-advocate")]
    public async Task<ActionResult<DevilsAdvocateResponse>> GetDevilsAdvocate(DevilsAdvocateRequest request)
    {
        var query = new GetDevilsAdvocateQuery
        {
            QuestionText = request.QuestionText,
            UserAnswer = request.UserAnswer
        };

        var result = await Mediator.Send(query);
        return Ok(new DevilsAdvocateResponse(result));
    }
}
