using Hangfire.Dashboard;

namespace DeepMatch.WebApi.Filters;


public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {

        var httpContext = context.GetHttpContext();
        if (httpContext == null) return false;

        // Проверяем если это Development окружение
        var isDevelopment = httpContext.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment();

        return isDevelopment;
    }
}
