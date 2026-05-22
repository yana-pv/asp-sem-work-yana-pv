using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using DeepMatch.Infrastructure.Data;

namespace DeepMatch.Infrastructure.Messaging.SignalR;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(AppDbContext context, ILogger<ChatHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task JoinMatchChat(string matchId)
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId) || !Guid.TryParse(matchId, out var parsedMatchId))
        {
            throw new HubException("Некорректный запрос на подключение к чату");
        }

        var hasAccess = await _context.Matches
            .AnyAsync(m => m.Id == parsedMatchId && (m.User1Id == userId || m.User2Id == userId));

        if (!hasAccess)
        {
            throw new HubException("Нет доступа к этому чату");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
    }

    public async Task LeaveMatchChat(string matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Пользователь {UserId} подключился к чату. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation(exception, "Пользователь {UserId} отключился от чата", userId);
        await base.OnDisconnectedAsync(exception);
    }
}
