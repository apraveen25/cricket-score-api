using CricketScore.Application.DTOs.Scoring;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Enums;
using CricketScore.Domain.Interfaces.Repositories;

namespace CricketScore.Application.Services;

public class LiveScoringService(
    IMatchRepository matchRepository,
    IInningsRepository inningsRepository,
    IDeliveryRepository deliveryRepository,
    ITeamRepository teamRepository,
    ScoringService scoringService)
{
    public async Task<LiveScoringStateResponse> GetLiveStateAsync(string matchId)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} not found.");

        if (match.CurrentInningsId is null)
            throw new InvalidOperationException("Match has no active innings.");

        var innings = await inningsRepository.GetByIdAsync(match.CurrentInningsId, matchId)
            ?? throw new InvalidOperationException("Active innings not found.");

        return await BuildLiveStateAsync(match, innings);
    }

    public async Task<LiveScoringStateResponse> RecordBallAsync(string matchId, BallDeliveryRequest request)
    {
        await scoringService.RecordDeliveryAsync(matchId, request);
        return await GetLiveStateAsync(matchId);
    }

    public async Task<LiveScoringStateResponse> UndoLastBallAsync(string matchId)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} not found.");

        if (match.CurrentInningsId is null)
            throw new InvalidOperationException("Match has no active innings.");

        var innings = await inningsRepository.GetByIdAsync(match.CurrentInningsId, matchId)
            ?? throw new InvalidOperationException("Active innings not found.");

        if (innings.IsCompleted)
            throw new InvalidOperationException("Cannot undo: innings is completed.");

        var lastDelivery = await deliveryRepository.GetLastByInningsAsync(innings.Id)
            ?? throw new InvalidOperationException("No deliveries to undo.");

        if (lastDelivery.PreDeliverySnapshot is null)
            throw new InvalidOperationException("Cannot undo: delivery has no snapshot.");

        RestoreFromSnapshot(innings, lastDelivery.PreDeliverySnapshot);

        await deliveryRepository.DeleteAsync(innings.Id, lastDelivery.Id);
        await inningsRepository.UpdateAsync(innings);

        return await BuildLiveStateAsync(match, innings);
    }

    public async Task<RunRateChartResponse> GetRunRateChartAsync(string matchId, string inningsId)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} not found.");

        var innings = await inningsRepository.GetByIdAsync(inningsId, matchId)
            ?? throw new KeyNotFoundException($"Innings {inningsId} not found.");

        _ = innings;

        var deliveries = (await deliveryRepository.GetByInningsAsync(inningsId))
            .OrderBy(d => d.OverNumber).ThenBy(d => d.Timestamp)
            .ToList();

        var overGroups = deliveries
            .GroupBy(d => d.OverNumber)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                OverNumber = g.Key + 1,
                RunsInOver = g.Sum(d => d.RunsScored + d.ExtraRuns),
                WicketsInOver = g.Count(d => d.IsWicket)
            })
            .ToList();

        var result = new List<OverRunRateDto>();
        int cumRuns = 0, cumWickets = 0, cumBalls = 0;

        foreach (var over in overGroups)
        {
            cumRuns += over.RunsInOver;
            cumWickets += over.WicketsInOver;
            cumBalls += 6;
            result.Add(new OverRunRateDto(
                over.OverNumber,
                over.RunsInOver,
                over.WicketsInOver,
                Math.Round(cumRuns * 6.0 / cumBalls, 2),
                cumRuns,
                cumWickets
            ));
        }

        return new RunRateChartResponse(matchId, inningsId, match.OversPerInnings, result);
    }

    public async Task<WagonWheelResponse> GetWagonWheelAsync(string matchId, string inningsId)
    {
        var shots = (await deliveryRepository.GetByInningsAsync(inningsId))
            .Where(d => d.ShotAngle.HasValue)
            .Select(d => new WagonWheelShotDto(
                d.ShotAngle!.Value,
                d.ShotDistance ?? 0.5,
                d.RunsScored,
                d.IsWicket,
                d.ExtraType != ExtraType.None
            ))
            .ToList();

        return new WagonWheelResponse(matchId, inningsId, shots);
    }

    private async Task<LiveScoringStateResponse> BuildLiveStateAsync(Domain.Entities.Match match, Innings innings)
    {
        var allInnings = (await inningsRepository.GetByMatchAsync(match.Id))
            .OrderBy(i => i.InningsNumber).ToList();

        var battingTeam = await teamRepository.GetByIdAsync(innings.BattingTeamId);
        var bowlingTeam = await teamRepository.GetByIdAsync(innings.BowlingTeamId);

        int currentOverNumber = innings.TotalLegalBalls / 6;
        var currentOverDeliveries = (await deliveryRepository.GetByInningsAndOverAsync(innings.Id, currentOverNumber))
            .OrderBy(d => d.Timestamp).ToList();

        int? target = null, runsNeeded = null, ballsRemaining = null;
        double? rrr = null;

        if (allInnings.Count == 2 && innings.InningsNumber == 2)
        {
            target = allInnings[0].TotalRuns + 1;
            runsNeeded = target.Value - innings.TotalRuns;
            ballsRemaining = match.OversPerInnings * 6 - innings.TotalLegalBalls;
            rrr = ballsRemaining > 0 ? Math.Round(runsNeeded.Value * 6.0 / ballsRemaining.Value, 2) : 0;
        }

        var crr = innings.TotalLegalBalls > 0
            ? Math.Round(innings.TotalRuns * 6.0 / innings.TotalLegalBalls, 2)
            : 0;

        var striker = BuildBatsmanDto(innings, innings.CurrentBatsmanId, isOnStrike: true);
        var nonStriker = BuildBatsmanDto(innings, innings.NonStrikeBatsmanId, isOnStrike: false);
        var currentBowler = BuildBowlerDto(innings, innings.CurrentBowlerId);

        var currentOverBalls = currentOverDeliveries
            .Select((d, i) => new CurrentOverBallDto(
                i,
                FormatBallDisplay(d),
                d.RunsScored,
                d.IsWicket,
                d.ExtraType != ExtraType.None,
                d.ExtraType))
            .ToList();

        var runsInCurrentOver = currentOverDeliveries.Sum(d => d.RunsScored + d.ExtraRuns);

        return new LiveScoringStateResponse(
            match.Id,
            innings.Id,
            innings.BattingTeamId,
            battingTeam?.Name ?? innings.BattingTeamId,
            innings.BowlingTeamId,
            bowlingTeam?.Name ?? innings.BowlingTeamId,
            innings.InningsNumber,
            innings.TotalRuns,
            innings.Wickets,
            FormatOvers(innings.TotalLegalBalls),
            target,
            runsNeeded,
            ballsRemaining,
            crr,
            rrr,
            striker,
            nonStriker,
            currentBowler,
            currentOverBalls,
            runsInCurrentOver,
            match.OversPerInnings,
            innings.IsCompleted
        );
    }

    private static LiveBatsmanDto? BuildBatsmanDto(Innings innings, string? playerId, bool isOnStrike)
    {
        if (playerId is null) return null;
        var b = innings.BattingScores.FirstOrDefault(b => b.PlayerId == playerId);
        if (b is null) return null;
        return new LiveBatsmanDto(
            b.PlayerId, b.PlayerName, isOnStrike,
            b.Runs, b.Balls, b.Fours, b.Sixes,
            b.Balls > 0 ? Math.Round(b.Runs * 100.0 / b.Balls, 2) : 0);
    }

    private static LiveBowlerDto? BuildBowlerDto(Innings innings, string? playerId)
    {
        if (playerId is null) return null;
        var bw = innings.BowlingScores.FirstOrDefault(b => b.PlayerId == playerId);
        if (bw is null) return null;
        var totalBalls = bw.Overs * 6 + bw.BallsInCurrentOver;
        return new LiveBowlerDto(
            bw.PlayerId, bw.PlayerName,
            $"{bw.Overs}.{bw.BallsInCurrentOver}",
            bw.Maidens, bw.Runs, bw.Wickets,
            totalBalls > 0 ? Math.Round(bw.Runs * 6.0 / totalBalls, 2) : 0);
    }

    private static void RestoreFromSnapshot(Innings innings, InningsStateSnapshot snapshot)
    {
        innings.TotalRuns = snapshot.TotalRuns;
        innings.Wickets = snapshot.Wickets;
        innings.TotalLegalBalls = snapshot.TotalLegalBalls;
        innings.Extras = snapshot.Extras;
        innings.IsCompleted = snapshot.IsCompleted;
        innings.CurrentBatsmanId = snapshot.CurrentBatsmanId;
        innings.NonStrikeBatsmanId = snapshot.NonStrikeBatsmanId;
        innings.CurrentBowlerId = snapshot.CurrentBowlerId;
        innings.BattingScores = snapshot.BattingScores;
        innings.BowlingScores = snapshot.BowlingScores;
        innings.FallOfWickets = snapshot.FallOfWickets;
    }

    private static string FormatBallDisplay(Delivery d)
    {
        if (d.IsWicket && d.ExtraType == ExtraType.None)
            return d.RunsScored > 0 ? $"{d.RunsScored}W" : "W";

        return d.ExtraType switch
        {
            ExtraType.Wide => "wd",
            ExtraType.NoBall => d.RunsScored > 0 ? $"nb+{d.RunsScored}" : "nb",
            ExtraType.Bye => $"B{d.ExtraRuns}",
            ExtraType.LegBye => $"LB{d.ExtraRuns}",
            _ => d.RunsScored.ToString()
        };
    }

    private static string FormatOvers(int totalLegalBalls) =>
        $"{totalLegalBalls / 6}.{totalLegalBalls % 6}";
}
