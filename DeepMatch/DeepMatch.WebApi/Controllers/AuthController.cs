using DeepMatch.Application.Common.Interfaces;
using DeepMatch.Application.Features.Auth.Commands.Login;
using DeepMatch.Application.Features.Auth.Commands.Register;
using DeepMatch.WebApi.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DeepMatch.WebApi.Controllers;

/// <summary>
/// Аутентификация и регистрация пользователей.
/// </summary>
public class AuthController : ApiControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Зарегистрировать нового пользователя.
    /// </summary>
    /// <param name="request">Email, имя, возраст и пароль пользователя.</param>
    /// <returns>JWT-токен и базовая информация о пользователе.</returns>
    /// <response code="200">Пользователь зарегистрирован.</response>
    /// <response code="400">Ошибка валидации или пользователь уже существует.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            UserName = request.UserName,
            Password = request.Password,
            Age = request.Age
        };

        var result = await Mediator.Send(command);
        var token = _tokenService.GenerateAccessToken(result.UserId, result.Email, result.UserName, result.Role);
        return Ok(new { result.UserId, result.Email, result.UserName, result.Role, Token = token });
    }

    /// <summary>
    /// Войти в систему.
    /// </summary>
    /// <param name="request">Email и пароль пользователя.</param>
    /// <returns>JWT-токен и базовая информация о пользователе.</returns>
    /// <response code="200">Вход выполнен.</response>
    /// <response code="400">Некорректный email или пароль.</response>
    /// <response code="429">Слишком много попыток входа.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimit")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await Mediator.Send(command);
        var token = _tokenService.GenerateAccessToken(result.UserId, result.Email, result.UserName, result.Role);
        return Ok(new { result.UserId, result.Email, result.UserName, result.Role, Token = token });
    }
}
