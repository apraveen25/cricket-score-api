using CricketScore.Application.DTOs.Scoring;
using CricketScore.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CricketScore.API.Hubs;

namespace CricketScore.API.Controllers;

/// <summary>
/// Dedicated controller for the live scoring UI.
/// Provides a single-call live state, ball recording with undo, and chart data.
/// </summary>
[ApiController]
[Route("api/v1/live/{matchId}")]
[Authorize]
public class LiveScoringController(
    LiveScoringService liveScoringService,
    IValidator<BallDeliveryRequest> ballValidator,
    IHubContext<ScoreHub> scoreHub) : ControllerBase
{
    /// <summary>
    /// Returns the full live scoring state for the UI — score, batsmen, bowler,
    /// current over ball sequence, CRR/RRR, and target info.
    /// </summary>
    [HttpGet("state")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLiveState(string matchId)
    {
        var state = await liveScoringService.GetLiveStateAsync(matchId);
        return Ok(state);
    }

    /// <summary>
    /// Records a ball delivery and returns the updated live state.
    /// Supports runs (0–6), extras (Wide, No ball, Bye, Leg bye), and wickets.
    /// Optional ShotAngle/ShotDistance for wagon wheel data.
    /// </summary>
    [HttpPost("ball")]
    public async Task<IActionResult> RecordBall(string matchId, [FromBody] BallDeliveryRequest request)
    {
        var validation = await ballValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var state = await liveScoringService.RecordBallAsync(matchId, request);

        await scoreHub.Clients.Group($"match-{matchId}")
            .SendAsync("LiveStateUpdated", state);

        return Ok(state);
    }

    /// <summary>
    /// Undoes the last recorded ball in the current innings.
    /// Restores the innings to its pre-delivery state using the stored snapshot.
    /// Not allowed once the innings is completed.
    /// </summary>
    [HttpPost("undo")]
    public async Task<IActionResult> UndoLastBall(string matchId)
    {
        var state = await liveScoringService.UndoLastBallAsync(matchId);

        await scoreHub.Clients.Group($"match-{matchId}")
            .SendAsync("LiveStateUpdated", state);

        return Ok(state);
    }

    /// <summary>
    /// Returns over-by-over run rate data for the run rate chart visualization.
    /// </summary>
    [HttpGet("{inningsId}/run-rate")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRunRateChart(string matchId, string inningsId)
    {
        var chart = await liveScoringService.GetRunRateChartAsync(matchId, inningsId);
        return Ok(chart);
    }

    /// <summary>
    /// Returns wagon wheel shot placement data (angle + distance per ball).
    /// Only includes deliveries where ShotAngle was provided during ball recording.
    /// </summary>
    [HttpGet("{inningsId}/wagon-wheel")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWagonWheel(string matchId, string inningsId)
    {
        var wheel = await liveScoringService.GetWagonWheelAsync(matchId, inningsId);
        return Ok(wheel);
    }
}
