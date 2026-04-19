using System.Security.Claims;
using CricketScore.Application.DTOs.Players;
using CricketScore.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CricketScore.API.Controllers;

[ApiController]
[Route("api/v1/players")]
public class PlayersController(PlayerService playerService, IValidator<CreatePlayerRequest> validator) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var player = await playerService.CreatePlayerAsync(request, UserId);
        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlayer(string id)
    {
        var player = await playerService.GetPlayerAsync(id);
        return Ok(player);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePlayer(string id, [FromBody] UpdatePlayerRequest request)
    {
        var player = await playerService.UpdatePlayerAsync(id, request, UserId);
        return Ok(player);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlayers([FromQuery] string? search)
    {
        var players = string.IsNullOrWhiteSpace(search)
            ? await playerService.GetAllPlayersAsync()
            : await playerService.SearchPlayersAsync(search);

        return Ok(players);
    }
}
