namespace DeepMatch.Application.Common.Interfaces;

public interface IAiService
{
    Task<string> GetDevilsAdvocateAsync(string questionText, string userAnswer);
    Task<string> GenerateIcebreakerAsync(List<string> user1Tags, List<string> user2Tags);
    Task<List<string>> AnalyzeAnswerTagsAsync(string answerText);
}
