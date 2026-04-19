using CricketScore.Application.DTOs.Auth;
using CricketScore.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CricketScore.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(AuthService authService, IValidator<RegisterRequest> registerValidator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var result = await authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return Ok(result);
    }
}
