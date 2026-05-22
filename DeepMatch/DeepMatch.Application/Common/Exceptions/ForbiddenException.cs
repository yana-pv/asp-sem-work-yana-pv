namespace DeepMatch.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Доступ запрещён") : base(message)
    {
    }
}
