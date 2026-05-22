using DeepMatch.Application.Features.Questions.Commands.CreateQuestion;
using DeepMatch.Application.Features.Questions.Commands.DeleteQuestion;
using DeepMatch.Application.Features.Questions.Queries.GetAllQuestions;
using DeepMatch.Domain.Constants;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Questions.Common;

namespace DeepMatch.WebApi.Controllers.Admin;

/// <summary>
/// Управление вопросами для администратора.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminQuestionsController : ApiControllerBase
{
    /// <summary>
    /// Получить все вопросы.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<QuestionAdminDto>>> GetQuestions()
    {
        return await Mediator.Send(new GetAllQuestionsQuery());
    }

    /// <summary>
    /// Создать вопрос.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateQuestion(CreateQuestionRequest request)
    {
        var id = await Mediator.Send(new CreateQuestionCommand(request.Text, request.Category));
        return CreatedAtAction(nameof(GetQuestions), new { id }, new { id });
    }

    /// <summary>
    /// Удалить вопрос.
    /// </summary>
    [HttpPost("{questionId:guid}/delete")]
    public async Task<ActionResult> DeleteQuestion(Guid questionId)
    {
        await Mediator.Send(new DeleteQuestionCommand(questionId));
        return Ok(new { message = "Вопрос удалён" });
    }
}
