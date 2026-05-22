using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Logging;
using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DeepMatch.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<MinioOptions> options, ILogger<FileStorageService> logger)
    {
        var minioOptions = options.Value;
        var endpoint = GetRequiredMinioSetting(minioOptions.Endpoint, nameof(MinioOptions.Endpoint));
        var accessKey = GetRequiredMinioSetting(minioOptions.AccessKey, nameof(MinioOptions.AccessKey));
        var secretKey = GetRequiredMinioSetting(minioOptions.SecretKey, nameof(MinioOptions.SecretKey));
        _bucketName = GetRequiredMinioSetting(minioOptions.BucketName, nameof(MinioOptions.BucketName));
        _logger = logger;

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();
    }

    public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
    {
        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucketName));

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs);
        _logger.LogInformation("Файл {FileName} загружен в MinIO", fileName);

        return fileName;
    }

    public async Task<Stream?> GetFileAsync(string fileName)
    {
        try
        {
            var memoryStream = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithCallbackStream(async (stream, cancellationToken) =>
                {
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                });

            await _minioClient.GetObjectAsync(args);

            memoryStream.Position = 0;
            return memoryStream.Length > 0 ? memoryStream : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Файл {FileName} не найден в MinIO", fileName);
            return null;
        }
    }

    private static string GetRequiredMinioSetting(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"MinIO:{key} must be configured.");
        }

        return value;
    }
}
