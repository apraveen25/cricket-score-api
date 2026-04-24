using System.Security.Claims;
using CricketScore.Application.DTOs.Teams;
using CricketScore.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CricketScore.API.Controllers;

[ApiController]
[Route("api/v1/teams")]
[Authorize]
public class TeamsController(TeamService teamService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var team = await teamService.CreateTeamAsync(request, UserId);
        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeam(string id)
    {
        var team = await teamService.GetTeamAsync(id);
        return Ok(team);
    }

    [HttpPost("{id}/players")]
    public async Task<IActionResult> AddPlayer(string id, [FromBody] AddPlayerRequest request)
    {
        var team = await teamService.AddPlayerAsync(id, request, UserId);
        return Ok(team);
    }

    [HttpPut("{id}/players/{playerId}")]
    public async Task<IActionResult> UpdatePlayer(string id, string playerId, [FromBody] UpdateTeamPlayerRequest request)
    {
        var team = await teamService.UpdateTeamPlayerAsync(id, playerId, request, UserId);
        return Ok(team);
    }

    [HttpDelete("{id}/players/{playerId}")]
    public async Task<IActionResult> RemovePlayer(string id, string playerId)
    {
        var team = await teamService.RemovePlayerAsync(id, playerId, UserId);
        return Ok(team);
    }

    [HttpPut("{id}/players/rearrange")]
    public async Task<IActionResult> RearrangePlayers(string id, [FromBody] RearrangePlayersRequest request)
    {
        var team = await teamService.RearrangePlayersAsync(id, request, UserId);
        return Ok(team);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTeams()
    {
        var teams = await teamService.GetMyTeamsAsync(UserId);
        return Ok(teams);
    }
}
