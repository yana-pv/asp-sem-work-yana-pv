using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace DeepMatch.Infrastructure.Hangfire;

public class MediatorHangfireBridge
{
    private readonly IServiceProvider _serviceProvider;

    public MediatorHangfireBridge(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Send<T>(T command) where T : IRequest
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        await mediator.Send(command);
    }
}
