namespace DeepMatch.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType);
    Task<Stream?> GetFileAsync(string fileName);
}
