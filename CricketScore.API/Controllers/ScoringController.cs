using CricketScore.Application.DTOs.Scoring;
using CricketScore.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CricketScore.API.Hubs;

namespace CricketScore.API.Controllers;

[ApiController]
[Route("api/v1/matches/{matchId}")]
[Authorize]
public class ScoringController(
    ScoringService scoringService,
    IValidator<BallDeliveryRequest> ballValidator,
    IHubContext<ScoreHub> scoreHub) : ControllerBase
{
    [HttpPost("ball")]
    public async Task<IActionResult> RecordBall(string matchId, [FromBody] BallDeliveryRequest request)
    {
        var validation = await ballValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var scorecard = await scoringService.RecordDeliveryAsync(matchId, request);

        await scoreHub.Clients.Group($"match-{matchId}")
            .SendAsync("ScorecardUpdated", scorecard);

        return Ok(scorecard);
    }

    [HttpGet("scorecard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetScorecard(string matchId)
    {
        var scorecard = await scoringService.GetScorecardAsync(matchId);
        return Ok(scorecard);
    }
}
