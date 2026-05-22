using System.Text;
using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Infrastructure;
using DeepMatch.Application;
using DeepMatch.Infrastructure.Data;
using DeepMatch.Infrastructure.Data.Seed;
using DeepMatch.WebApi.Middleware;
using DeepMatch.Infrastructure.Messaging.SignalR;
using DeepMatch.Infrastructure.Hangfire;
using DeepMatch.WebApi.Constants;
using Serilog;
using Serilog.Events;
using DeepMatch.WebApi.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: WebApiDefaults.RetainedLogFileCountLimit,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Запуск приложения DeepMatch");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // JWT АУТЕНТИФИКАЦИЯ 
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtKey = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    // RATE LIMITING ДЛЯ ЗАЩИТЫ ОТ ПЕРЕБОРА ПАРОЛЕЙ
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("AuthLimit", config =>
        {
            config.PermitLimit = WebApiDefaults.AuthRateLimitPermitLimit;
            config.Window = TimeSpan.FromMinutes(WebApiDefaults.AuthRateLimitWindowMinutes);
            config.QueueLimit = WebApiDefaults.AuthRateLimitQueueLimit;
            config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    });

    // СЛОИ ПРИЛОЖЕНИЯ
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddScoped<IProfilePhotoUrlService, ProfilePhotoUrlService>();

    // CONTROLLERS 
    builder.Services.AddControllers();

    // SWAGGER
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "DeepMatch API",
            Version = "v1",
            Description = "API платформы знакомств по глубине мышления: вопросы дня, ответы, свайпы, мэтчи, чат, уведомления, бейджи и AI-помощник."
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // HANGFIRE DASHBOARD
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new DeepMatch.WebApi.Filters.HangfireAuthorizationFilter() }
    });

    // СИДИРОВАНИЕ 
    await SeedData.Initialize(app.Services);

    // Запуск фоновых задач Hangfire
    using (var scope = app.Services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        HangfireJobScheduler.ScheduleJobs(recurringJobManager);
    }

    // MIDDLEWARE 
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // SECURITY HEADERS
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Статические файлы 
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();

    app.UseAuthentication();
    app.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdValue, out var userId))
            {
                var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
                var isBlocked = await dbContext.Users.AnyAsync(u => u.Id == userId && u.IsBlocked);

                if (isBlocked)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { detail = "Ваш аккаунт заблокирован" });
                    return;
                }
            }
        }

        await next();
    });
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<ChatHub>("/chatHub");

    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось с ошибкой");
}
finally
{
    Log.CloseAndFlush();
}
