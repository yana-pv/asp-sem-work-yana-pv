using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Domain.Constants;
using DeepMatch.Infrastructure.Options;
using Microsoft.Extensions.Options;


namespace DeepMatch.Infrastructure.Services.Gemini;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiAiService> _logger;

    public GeminiAiService(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiAiService> logger)
    {
        _httpClient = httpClient;
        var geminiOptions = options.Value;
        _apiKey = geminiOptions.ApiKey ?? string.Empty;
        _model = GetRequiredGeminiSetting(geminiOptions.Model, nameof(GeminiOptions.Model));
        var baseUrl = GetRequiredGeminiSetting(geminiOptions.BaseUrl, nameof(GeminiOptions.BaseUrl));
        _httpClient.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
        _logger = logger;
    }

    // Адвокат дьявола 
    public async Task<string> GetDevilsAdvocateAsync(string questionText, string userAnswer)
    {
        var prompt = $@"
            Вопрос: {questionText}

            Пользователь дал такой ответ: {userAnswer}

            Ты — 'адвокат дьявола'. Предложи убедительную альтернативную точку зрения на этот вопрос. 
            Не говори, что ответ пользователя неправильный. Просто покажи другую сторону.
            Ответь на русском языке, {BusinessRules.Ai.MinDevilsAdvocateSentences}-{BusinessRules.Ai.MaxDevilsAdvocateSentences} предложения.";

        try
        {
            return await CallGeminiAsync(prompt);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации ответа адвоката дьявола");
            return "Не удалось сгенерировать ответ адвоката дьявола. Попробуйте позже.";
        }
    }

    // Ледокол
    public async Task<string> GenerateIcebreakerAsync(List<string> user1Tags, List<string> user2Tags)
    {
        var tags1 = string.Join(", ", user1Tags.Take(BusinessRules.Ai.MaxTagsPerUserInIcebreaker));
        var tags2 = string.Join(", ", user2Tags.Take(BusinessRules.Ai.MaxTagsPerUserInIcebreaker));
        
        var prompt = $@"
            Два человека познакомились на платформе для глубокого общения. 
            Интересы первого: {tags1}. Интересы второго: {tags2}.
            Придумай один интересный, открытый вопрос для начала разговора, который поможет им раскрыться.
            Ответь на русском языке, только вопрос, без вступления.";

        try
        {
            return await CallGeminiAsync(prompt);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации ледокола");
            return "Расскажите о себе что-нибудь интересное?";
        }
    }

    // Анализ тегов ответа 
    public async Task<List<string>> AnalyzeAnswerTagsAsync(string answerText)
    {
        var prompt = $@"
            Проанализируй следующий ответ пользователя и присвой ему {BusinessRules.Ai.MinTagsPerAnswer}-{BusinessRules.Ai.MaxTagsPerAnswer} тега из списка:
            [логика, эмпатия, юмор, философия, критическое_мышление, эмоциональность, прагматизм, идеализм, креативность, рациональность]

            Ответ пользователя: {answerText}

            Верни ТОЛЬКО теги через запятую, без пояснений. Пример: логика, философия, креативность";

        try
        {
            var result = await CallGeminiAsync(prompt);
            return result.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе тегов ответа");
            return new List<string>();
        }
    }

    private async Task<string> CallGeminiAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API key не настроен");
            return "AI недоступен. Свяжитесь с администратором.";
        }

        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"models/{_model}:generateContent?key={_apiKey}")
            {
                Content = JsonContent.Create(requestBody)
            };

            _logger.LogInformation("Запрос к Gemini API");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ошибка Gemini API: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return "Ошибка при обращении к AI сервису.";
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);

            var text = geminiResponse?.Candidates?.FirstOrDefault()
                ?.Content?.Parts?.FirstOrDefault()
                ?.Text ?? "Не удалось получить ответ от AI";

            _logger.LogInformation("Ответ от Gemini получен: {Length} символов", text.Length);

            return text;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при вызове Gemini API");
            return "Ошибка при обращении к AI сервису.";
        }
    }

    private static string GetRequiredGeminiSetting(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Gemini:{key} must be configured.");
        }

        return value;
    }
}


