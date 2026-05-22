using FluentValidation.Results;

namespace DeepMatch.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public List<ValidationError> Errors { get; }

    public ValidationException(List<ValidationFailure> failures) : base("Ошибка валидации")
    {
        Errors = failures.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage)).ToList();
    }
}

public class ValidationError
{
    public string Property { get; }
    public string Message { get; }

    public ValidationError(string property, string message)
    {
        Property = property;
        Message = message;
    }
}
