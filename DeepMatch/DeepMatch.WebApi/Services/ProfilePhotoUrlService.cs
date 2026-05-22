using DeepMatch.Application.Common.Interfaces;
using DeepMatch.WebApi.Constants;

namespace DeepMatch.WebApi.Services;

public class ProfilePhotoUrlService : IProfilePhotoUrlService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;

    public ProfilePhotoUrlService(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
    {
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
    }

    public string GetProfilePhotoUrl(Guid photoId)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Current HTTP context is required to generate profile photo URLs.");

        var path = _linkGenerator.GetPathByName(
            httpContext,
            ApiRouteNames.GetProfilePhoto,
            new { photoId });

        return path ?? throw new InvalidOperationException($"Route '{ApiRouteNames.GetProfilePhoto}' is not registered.");
    }
}
