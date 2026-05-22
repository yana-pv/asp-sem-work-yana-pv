using MediatR;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Interfaces;

namespace DeepMatch.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger, ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId.ToString() : "anonymous";

        _logger.LogInformation("Выполнение {RequestName} пользователем {UserId}", requestName, userId);

        var response = await next();

        _logger.LogInformation("Завершено {RequestName} пользователем {UserId}", requestName, userId);

        return response;
    }
}
