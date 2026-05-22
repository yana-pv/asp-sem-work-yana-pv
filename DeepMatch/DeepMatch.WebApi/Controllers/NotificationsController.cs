using DeepMatch.Application.Features.Notifications.Commands.MarkAsRead;
using DeepMatch.Application.Features.Notifications.Queries.GetNotifications;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeepMatch.Application.Features.Notifications.Common;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Уведомления пользователя: сообщения, мэтчи, бейджи, рейтинг и вопрос дня.
/// </summary>
[Authorize]
public class NotificationsController : ApiControllerBase
{
    /// <summary>
    /// Получить последние уведомления текущего пользователя.
    /// </summary>
    /// <returns>Список последних уведомлений.</returns>
    /// <response code="200">Уведомления получены.</response>
    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications()
    {
        return await Mediator.Send(new GetNotificationsQuery());
    }

    /// <summary>
    /// Отметить уведомления прочитанными.
    /// </summary>
    /// <param name="request">ID уведомления или null, чтобы отметить все непрочитанные.</param>
    /// <response code="200">Уведомления отмечены прочитанными.</response>
    [HttpPost("read")]
    public async Task<ActionResult> MarkAsRead(MarkNotificationReadRequest request)
    {
        await Mediator.Send(new MarkAsReadCommand(request.NotificationId));
        return Ok();
    }
}
