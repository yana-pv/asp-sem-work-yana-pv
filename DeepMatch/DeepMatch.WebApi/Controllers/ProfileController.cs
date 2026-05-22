using DeepMatch.Application.Features.Profile.Common;
using DeepMatch.Application.Features.Profile.Commands.UpdateProfile;
using DeepMatch.Application.Features.Profile.Commands.UploadAvatar;
using DeepMatch.Application.Features.Profile.Commands.UploadProfilePhoto;
using DeepMatch.Application.Features.Profile.Queries.GetAvatar;
using DeepMatch.Application.Features.Profile.Queries.GetProfile;
using DeepMatch.Application.Features.Profile.Queries.GetProfilePhoto;
using DeepMatch.Application.Features.Profile.Queries.GetPublicProfile;
using DeepMatch.Domain.Constants;
using DeepMatch.WebApi.Constants;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Профиль пользователя, его ответы, бейджи, аватар и фотографии.
/// </summary>
[Authorize]
public class ProfileController : ApiControllerBase
{
    /// <summary>
    /// Получить профиль текущего пользователя.
    /// </summary>
    /// <returns>Данные профиля, рейтинг, бейджи, фото и портфолио ответов.</returns>
    /// <response code="200">Профиль найден.</response>
    /// <response code="404">Профиль не найден.</response>
    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var profile = await Mediator.Send(new GetProfileQuery());

        if (profile == null)
        {
            return NotFound(new { message = "Профиль не найден" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Обновить информацию о себе в профиле текущего пользователя.
    /// </summary>
    /// <param name="request">Текст поля "О себе".</param>
    /// <response code="200">Профиль обновлен.</response>
    [HttpPut]
    public async Task<ActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        await Mediator.Send(new UpdateProfileCommand(request.Bio));
        return Ok(new { message = "Профиль обновлён" });
    }

    /// <summary>
    /// Получить публичный профиль пользователя, с которым есть мэтч.
    /// </summary>
    /// <param name="userId">ID собеседника.</param>
    /// <returns>Публичная информация профиля.</returns>
    [HttpGet("public/{userId:guid}")]
    public async Task<ActionResult<PublicProfileDto>> GetPublicProfile(Guid userId)
    {
        return await Mediator.Send(new GetPublicProfileQuery(userId));
    }

    /// <summary>
    /// Загрузить или заменить аватар текущего пользователя.
    /// </summary>
    /// <param name="file">Файл изображения размером до 5 МБ.</param>
    /// <returns>Ссылка на сохраненный аватар.</returns>
    /// <response code="200">Аватар загружен.</response>
    /// <response code="400">Файл не выбран или превышает допустимый размер.</response>
    [HttpPost("avatar")]
    public async Task<ActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Файл не выбран" });
        }

        if (file.Length > BusinessRules.Files.MaxImageUploadSizeBytes)
        {
            return BadRequest(new { message = $"Файл слишком большой (макс. {BusinessRules.Files.MaxImageUploadSizeMegabytes} МБ)" });
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Можно загружать только изображения" });
        }

        var command = new UploadAvatarCommand
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var url = await Mediator.Send(command);
        return Ok(new { avatarUrl = url });
    }

    /// <summary>
    /// Загрузить дополнительное фото в профиль.
    /// </summary>
    /// <param name="file">Файл изображения размером до 5 МБ.</param>
    /// <returns>Информация о загруженном фото.</returns>
    [HttpPost("photos")]
    public async Task<ActionResult<ProfilePhotoDto>> UploadPhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Файл не выбран" });
        }

        if (file.Length > BusinessRules.Files.MaxImageUploadSizeBytes)
        {
            return BadRequest(new { message = $"Файл слишком большой (макс. {BusinessRules.Files.MaxImageUploadSizeMegabytes} МБ)" });
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Можно загружать только изображения" });
        }

        var command = new UploadProfilePhotoCommand
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var photo = await Mediator.Send(command);
        return Ok(photo);
    }

    /// <summary>
    /// Получить фото профиля по ID.
    /// </summary>
    /// <param name="photoId">ID фото.</param>
    /// <returns>Файл изображения.</returns>
    [HttpGet("photos/{photoId:guid}", Name = ApiRouteNames.GetProfilePhoto)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhoto(Guid photoId)
    {
        var result = await Mediator.Send(new GetProfilePhotoQuery(photoId));

        if (result == null)
        {
            return NotFound(new { message = "Фото не найдено" });
        }

        return File(result.FileStream, result.ContentType);
    }

    /// <summary>
    /// Получить аватар пользователя по ID.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <returns>Файл аватара или стандартный SVG-аватар.</returns>
    /// <response code="200">Аватар получен.</response>
    [HttpGet("avatar/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvatar(Guid userId)
    {
        var result = await Mediator.Send(new GetAvatarQuery(userId));

        if (result == null)
        {
            return File(GetDefaultAvatar(), "image/svg+xml");
        }

        return File(result.FileStream, result.ContentType);
    }

    private static byte[] GetDefaultAvatar()
    {
        var svg = @"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'>
            <rect width='100' height='100' rx='50' fill='#cba1d4'/>
            <circle cx='50' cy='38' r='16' fill='#ffffcd'/>
            <path d='M22 84c5-20 19-30 28-30s23 10 28 30' fill='#ffffcd'/>
        </svg>";
        return System.Text.Encoding.UTF8.GetBytes(svg);
    }
}
