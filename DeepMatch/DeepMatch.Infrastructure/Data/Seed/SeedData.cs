using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;
using DeepMatch.Domain.Enums;
using DeepMatch.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeepMatch.Infrastructure.Data.Seed;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seedAdminOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedAdminOptions>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SeedData));

        await context.Database.MigrateAsync();

        if (await context.Questions.AnyAsync())
            return;

        var badges = new List<Badge>
        {
            new() { Id = Guid.NewGuid(), Name = "Логик", Description = $"Более {BusinessRules.Badges.TaggedAnswersRequired} ответов с тегом «логика»", Type = BadgeType.Logician },
            new() { Id = Guid.NewGuid(), Name = "Эмпат", Description = $"Более {BusinessRules.Badges.TaggedAnswersRequired} ответов с тегом «эмпатия»", Type = BadgeType.Empath },
            new() { Id = Guid.NewGuid(), Name = "Эрудит", Description = $"Ответил на вопросы из {BusinessRules.Badges.DistinctCategoriesRequired} разных категорий", Type = BadgeType.Erudite },
            new() { Id = Guid.NewGuid(), Name = "Мастер дебатов", Description = $"Более {BusinessRules.Badges.LikesRequired} лайков на ответах", Type = BadgeType.Debater },
            new() { Id = Guid.NewGuid(), Name = "Активный участник", Description = $"Отвечал {BusinessRules.Badges.ConsecutiveAnswerDaysRequired} дней подряд", Type = BadgeType.Active },
            new() { Id = Guid.NewGuid(), Name = "Создатель связей", Description = $"Более {BusinessRules.Badges.MatchesRequired} взаимных мэтчей", Type = BadgeType.Matchmaker }
        };
        context.Badges.AddRange(badges);

        // Админ
        if (!await context.Users.AnyAsync(u => u.Role == UserRoles.Admin))
        {
            if (string.IsNullOrWhiteSpace(seedAdminOptions.Email) ||
                string.IsNullOrWhiteSpace(seedAdminOptions.Password))
            {
                logger.LogInformation("Seed admin is not configured; admin user was not created.");
            }
            else
            {
                var passwordHasher = new PasswordHasher<User>();
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = seedAdminOptions.Email,
                    UserName = string.IsNullOrWhiteSpace(seedAdminOptions.UserName)
                        ? seedAdminOptions.Email
                        : seedAdminOptions.UserName,
                    Age = BusinessRules.Users.SeedAdminAge,
                    Role = UserRoles.Admin,
                    RegisteredAt = DateTime.UtcNow
                };
                admin.PasswordHash = passwordHasher.HashPassword(admin, seedAdminOptions.Password);
                context.Users.Add(admin);
            }
        }

        // Системный пользователь
        var systemUserId = SystemUsers.DeepMatchUserId;
        if (!await context.Users.AnyAsync(u => u.Id == systemUserId))
        {
            var systemUser = new User
            {
                Id = systemUserId,
                Email = "system@deepmatch.local",
                UserName = "DeepMatch",
                Age = BusinessRules.Users.SystemUserAge,
                Role = UserRoles.System,
                Bio = "Системный пользователь",
                RegisteredAt = DateTime.UtcNow,
                PasswordHash = "NOT_FOR_LOGIN",
                IsBlocked = true
            };
            context.Users.Add(systemUser);
            await context.SaveChangesAsync();
        }

        var questions = new List<Question>
        {
            new() { Id = Guid.NewGuid(), Text = "Что важнее: быть правым или быть добрым?", Category = QuestionCategory.Ethics },
            new() { Id = Guid.NewGuid(), Text = "Если бы вы могли прожить один день заново, что бы вы изменили?", Category = QuestionCategory.Life },
            new() { Id = Guid.NewGuid(), Text = "Существует ли судьба или мы сами создаём свою реальность?", Category = QuestionCategory.Philosophy },
            new() { Id = Guid.NewGuid(), Text = "Что для вас означает настоящая близость между людьми?", Category = QuestionCategory.Relationships },
            new() { Id = Guid.NewGuid(), Text = "Может ли ложь быть оправданной?", Category = QuestionCategory.Ethics },
            new() { Id = Guid.NewGuid(), Text = "Что бы вы сказали себе 10 лет назад?", Category = QuestionCategory.SelfDiscovery },
            new() { Id = Guid.NewGuid(), Text = "Свобода — это ответственность или вседозволенность?", Category = QuestionCategory.Philosophy },
            new() { Id = Guid.NewGuid(), Text = "Что делает человека по-настоящему счастливым?", Category = QuestionCategory.Life },
            new() { Id = Guid.NewGuid(), Text = "Может ли искусственный интеллект понять человеческие эмоции?", Category = QuestionCategory.Society },
            new() { Id = Guid.NewGuid(), Text = "Что вы цените в людях больше всего?", Category = QuestionCategory.Relationships },
            new() { Id = Guid.NewGuid(), Text = "Есть ли разница между одиночеством и уединением?", Category = QuestionCategory.SelfDiscovery },
            new() { Id = Guid.NewGuid(), Text = "Должны ли традиции меняться со временем?", Category = QuestionCategory.Society },
            new() { Id = Guid.NewGuid(), Text = "Что страшнее: неудача или отсутствие попытки?", Category = QuestionCategory.Life },
            new() { Id = Guid.NewGuid(), Text = "Можно ли простить предательство?", Category = QuestionCategory.Ethics },
            new() { Id = Guid.NewGuid(), Text = "Какую книгу или фильм вы бы посоветовали каждому?", Category = QuestionCategory.SelfDiscovery }
        };
        context.Questions.AddRange(questions);

        await context.SaveChangesAsync();

        // Назначить первый вопрос вопросом дня на сегодня
        var firstQuestion = questions.First();
        firstQuestion.DateOfDay = DateOnly.FromDateTime(DateTime.UtcNow);

        await context.SaveChangesAsync();
    }
}
