using DeepMatch.Application.Features.Chat.Commands.GenerateIcebreaker;
using DeepMatch.Application.Features.Chat.Commands.MarkMessagesAsRead;
using DeepMatch.Application.Features.Chat.Commands.SendMessage;
using DeepMatch.Application.Features.Chat.Queries.GetChatMatchInfo;
using DeepMatch.Application.Features.Chat.Queries.GetChatMessages;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Chat.Common;
using DeepMatch.WebApi.Contracts.Responses;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Чат между пользователями после взаимного мэтча.
/// </summary>
[Authorize]
public class ChatController : ApiControllerBase
{
    /// <summary>
    /// Отправить сообщение в чат мэтча.
    /// </summary>
    /// <param name="request">ID мэтча и текст сообщения.</param>
    /// <returns>Созданное сообщение.</returns>
    /// <response code="200">Сообщение отправлено.</response>
    /// <response code="403">Пользователь не участвует в этом мэтче.</response>
    /// <response code="404">Мэтч не найден.</response>
    [HttpPost("send")]
    public async Task<ActionResult<MessageDto>> SendMessage(SendMessageRequest request)
    {
        var command = new SendMessageCommand
        {
            MatchId = request.MatchId,
            Content = request.Content
        };

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Получить историю сообщений мэтча.
    /// </summary>
    /// <param name="matchId">ID мэтча.</param>
    /// <returns>Список сообщений в хронологическом порядке.</returns>
    /// <response code="200">История сообщений получена.</response>
    /// <response code="403">Пользователь не участвует в этом мэтче.</response>
    /// <response code="404">Мэтч не найден.</response>
    [HttpGet("{matchId}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(Guid matchId)
    {
        var result = await Mediator.Send(new GetChatMessagesQuery(matchId));
        return Ok(result);
    }

    /// <summary>
    /// Пометить входящие сообщения мэтча прочитанными.
    /// </summary>
    /// <param name="matchId">ID мэтча.</param>
    /// <response code="200">Сообщения помечены прочитанными.</response>
    /// <response code="403">Пользователь не участвует в этом мэтче.</response>
    /// <response code="404">Мэтч не найден.</response>
    [HttpPost("{matchId}/read")]
    public async Task<ActionResult> MarkMessagesAsRead(Guid matchId)
    {
        await Mediator.Send(new MarkMessagesAsReadCommand(matchId));
        return Ok();
    }

    /// <summary>
    /// Получить контекст чата: имена участников и ответы, из-за которых произошел мэтч.
    /// </summary>
    /// <param name="matchId">ID мэтча.</param>
    /// <returns>Информация для шапки и контекста чата.</returns>
    /// <response code="200">Информация о чате получена.</response>
    /// <response code="403">Пользователь не участвует в этом мэтче.</response>
    /// <response code="404">Мэтч не найден.</response>
    [HttpGet("{matchId}/info")]
    public async Task<ActionResult<ChatInfoDto>> GetMatchInfo(Guid matchId)
    {
        var result = await Mediator.Send(new GetChatInfoQuery(matchId));
        return Ok(result);
    }

    /// <summary>
    /// Сгенерировать AI-ледокол для продолжения диалога.
    /// </summary>
    /// <param name="matchId">ID мэтча.</param>
    /// <returns>Вопрос для начала или продолжения разговора.</returns>
    /// <response code="200">Ледокол сгенерирован.</response>
    /// <response code="403">Пользователь не участвует в этом мэтче.</response>
    /// <response code="404">Мэтч не найден.</response>
    [HttpPost("{matchId}/icebreaker")]
    public async Task<ActionResult<IcebreakerResponse>> GenerateIcebreaker(Guid matchId)
    {
        var result = await Mediator.Send(new GenerateIcebreakerCommand { MatchId = matchId });
        return Ok(new IcebreakerResponse(result));
    }
}
