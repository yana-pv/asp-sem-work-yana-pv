namespace DeepMatch.Application.Features.Profile.Queries.GetAvatar;

public record AvatarResult(Stream FileStream, string ContentType, string FileName);
