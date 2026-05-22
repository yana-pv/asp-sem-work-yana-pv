using DeepMatch.Application.Features.Answers.Commands.CreateAnswer;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Answers.Common;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Ответы пользователей на вопросы дня.
/// </summary>
[Authorize]
public class AnswersController : ApiControllerBase
{
    /// <summary>
    /// Создать ответ на вопрос.
    /// </summary>
    /// <param name="request">ID вопроса и текст ответа.</param>
    /// <returns>Созданный ответ.</returns>
    /// <response code="201">Ответ создан.</response>
    /// <response code="400">Ошибка валидации или повторный ответ на тот же вопрос.</response>
    /// <response code="404">Вопрос не найден.</response>
    [HttpPost]
    public async Task<ActionResult<AnswerDto>> CreateAnswer(CreateAnswerRequest request)
    {
        var command = new CreateAnswerCommand
        {
            QuestionId = request.QuestionId,
            Text = request.Text
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(CreateAnswer), new { id = result.Id }, result);
    }
}
