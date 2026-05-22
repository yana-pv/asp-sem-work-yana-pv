namespace DeepMatch.Application.Common.Exceptions;

public class DailyLimitExceededException : Exception
{
    public DailyLimitExceededException(int limit) : base($"Дневной лимит свайпов ({limit}) исчерпан")
    {
    }
}
