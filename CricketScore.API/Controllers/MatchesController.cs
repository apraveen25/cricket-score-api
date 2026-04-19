using System.Security.Claims;
using CricketScore.Application.DTOs.Matches;
using CricketScore.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CricketScore.API.Controllers;

[ApiController]
[Route("api/v1/matches")]
[Authorize]
public class MatchesController(MatchService matchService, IValidator<CreateMatchRequest> createMatchValidator) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
    {
        var validation = await createMatchValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var match = await matchService.CreateMatchAsync(request, UserId);
        return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, match);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMatch(string id)
    {
        var match = await matchService.GetMatchAsync(id);
        return Ok(match);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartMatch(string id, [FromBody] StartMatchRequest request)
    {
        var match = await matchService.StartMatchAsync(id, request, UserId);
        return Ok(match);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetRecentMatches()
    {
        var matches = await matchService.GetRecentMatchesAsync();
        return Ok(matches);
    }
}
