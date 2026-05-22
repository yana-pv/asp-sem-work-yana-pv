namespace DeepMatch.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key) : base($"Сущность '{entityName}' с ключом ({key}) не найдена")
    {
    }
}
