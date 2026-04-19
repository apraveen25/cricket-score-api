using CricketScore.Application.DTOs.Scoring;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Enums;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Domain.Interfaces.Services;

namespace CricketScore.Application.Services;

public class ScoringService(
    IMatchRepository matchRepository,
    IInningsRepository inningsRepository,
    IDeliveryRepository deliveryRepository,
    INotificationService notificationService)
{
    public async Task<ScorecardResponse> RecordDeliveryAsync(string matchId, BallDeliveryRequest request)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} not found.");

        if (match.Status != MatchStatus.Live)
            throw new InvalidOperationException("Match is not live.");

        var innings = await inningsRepository.GetByIdAsync(match.CurrentInningsId!, matchId)
            ?? throw new InvalidOperationException("No active innings found.");

        if (innings.IsCompleted)
            throw new InvalidOperationException("Current innings is already completed.");

        var delivery = new Delivery
        {
            InningsId = innings.Id,
            MatchId = matchId,
            OverNumber = innings.TotalLegalBalls / 6,
            BallInOver = CalculateBallInOver(innings),
            BatsmanId = request.BatsmanId,
            BowlerId = request.BowlerId,
            RunsScored = request.RunsScored,
            ExtraType = request.ExtraType,
            ExtraRuns = request.ExtraRuns,
            IsWicket = request.IsWicket,
            WicketType = request.WicketType,
            FielderId = request.FielderId,
            DismissedBatsmanId = request.DismissedBatsmanId
        };

        await deliveryRepository.CreateAsync(delivery);
        ApplyDeliveryToInnings(innings, delivery, request, match);

        var isLegalBall = request.ExtraType is ExtraType.None or ExtraType.Bye or ExtraType.LegBye;
        if (isLegalBall && innings.TotalLegalBalls >= match.OversPerInnings * 6)
            innings.IsCompleted = true;

        if (innings.Wickets >= 10)
            innings.IsCompleted = true;

        if (innings.IsCompleted)
            await HandleInningsCompletionAsync(match, innings);

        await inningsRepository.UpdateAsync(innings);

        var scorecard = await BuildScorecardAsync(matchId);

        await notificationService.PublishMatchEventAsync(matchId, "DeliveryRecorded", new
        {
            matchId,
            inningsId = innings.Id,
            runs = request.RunsScored + request.ExtraRuns,
            isWicket = request.IsWicket,
            totalRuns = innings.TotalRuns,
            wickets = innings.Wickets
        });

        return scorecard;
    }

    public async Task<ScorecardResponse> GetScorecardAsync(string matchId)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} not found.");

        _ = match;
        return await BuildScorecardAsync(matchId);
    }

    private static void ApplyDeliveryToInnings(Innings innings, Delivery delivery, BallDeliveryRequest request, Match match)
    {
        var totalRuns = request.RunsScored + request.ExtraRuns;
        innings.TotalRuns += totalRuns;
        innings.Extras += request.ExtraRuns;

        var isLegalBall = request.ExtraType is ExtraType.None or ExtraType.Bye or ExtraType.LegBye;
        if (isLegalBall)
            innings.TotalLegalBalls++;

        UpdateBatsmanScore(innings, delivery, request);
        UpdateBowlerScore(innings, delivery, request, isLegalBall);

        if (request.IsWicket && request.DismissedBatsmanId is not null)
        {
            innings.Wickets++;
            RecordFallOfWicket(innings, request.DismissedBatsmanId);

            if (request.NextBatsmanId is not null)
            {
                var isStrikeBatsman = request.DismissedBatsmanId == innings.CurrentBatsmanId;
                if (isStrikeBatsman)
                {
                    innings.CurrentBatsmanId = request.NextBatsmanId;
                    AddBatsmanIfNew(innings, request.NextBatsmanId);
                }
                else
                {
                    innings.NonStrikeBatsmanId = request.NextBatsmanId;
                    AddBatsmanIfNew(innings, request.NextBatsmanId);
                }
            }
        }

        RotateStrike(innings, request.RunsScored, request.ExtraType, isLegalBall);

        if (isLegalBall && innings.TotalLegalBalls % 6 == 0 && request.NextBowlerId is not null)
            innings.CurrentBowlerId = request.NextBowlerId;
    }

    private static void UpdateBatsmanScore(Innings innings, Delivery delivery, BallDeliveryRequest request)
    {
        if (request.ExtraType is ExtraType.Wide)
            return;

        var batsman = innings.BattingScores.FirstOrDefault(b => b.PlayerId == delivery.BatsmanId);
        if (batsman is null) return;

        batsman.Balls++;
        batsman.Runs += request.RunsScored;
        if (request.RunsScored == 4) batsman.Fours++;
        if (request.RunsScored == 6) batsman.Sixes++;

        if (request.IsWicket && request.DismissedBatsmanId == delivery.BatsmanId)
        {
            batsman.IsOut = true;
            batsman.DismissalDescription = BuildDismissalDescription(request);
        }
    }

    private static void UpdateBowlerScore(Innings innings, Delivery delivery, BallDeliveryRequest request, bool isLegalBall)
    {
        var bowler = innings.BowlingScores.FirstOrDefault(b => b.PlayerId == delivery.BowlerId);
        if (bowler is null)
        {
            var team = innings.BowlingTeamId;
            bowler = new BowlerScore { PlayerId = delivery.BowlerId, PlayerName = delivery.BowlerId };
            innings.BowlingScores.Add(bowler);
        }

        bowler.Runs += request.RunsScored + request.ExtraRuns;

        if (request.ExtraType == ExtraType.Wide) bowler.Wides++;
        else if (request.ExtraType == ExtraType.NoBall) bowler.NoBalls++;

        if (isLegalBall)
        {
            bowler.BallsInCurrentOver++;
            if (bowler.BallsInCurrentOver == 6)
            {
                bowler.Overs++;
                bowler.BallsInCurrentOver = 0;
                if (bowler.Runs == 0) bowler.Maidens++;
            }
        }

        if (request.IsWicket && request.WicketType is not WicketType.RunOut)
            bowler.Wickets++;
    }

    private static void RotateStrike(Innings innings, int runsScored, ExtraType extraType, bool isLegalBall)
    {
        if (extraType == ExtraType.Wide) return;

        var oddRuns = runsScored % 2 == 1;
        if (oddRuns)
            (innings.CurrentBatsmanId, innings.NonStrikeBatsmanId) = (innings.NonStrikeBatsmanId, innings.CurrentBatsmanId);

        if (isLegalBall && innings.TotalLegalBalls % 6 == 0)
            (innings.CurrentBatsmanId, innings.NonStrikeBatsmanId) = (innings.NonStrikeBatsmanId, innings.CurrentBatsmanId);
    }

    private static void RecordFallOfWicket(Innings innings, string dismissedBatsmanId)
    {
        var batsman = innings.BattingScores.FirstOrDefault(b => b.PlayerId == dismissedBatsmanId);
        innings.FallOfWickets.Add(new FallOfWicket
        {
            WicketNumber = innings.Wickets,
            PlayerId = dismissedBatsmanId,
            PlayerName = batsman?.PlayerName ?? dismissedBatsmanId,
            RunsAtFall = innings.TotalRuns,
            Over = FormatOver(innings.TotalLegalBalls)
        });
    }

    private static void AddBatsmanIfNew(Innings innings, string batsmanId)
    {
        if (!innings.BattingScores.Any(b => b.PlayerId == batsmanId))
        {
            innings.BattingScores.Add(new BatsmanScore
            {
                PlayerId = batsmanId,
                PlayerName = batsmanId,
                HasBatted = true,
                IsOnStrike = false
            });
        }
    }

    private static string BuildDismissalDescription(BallDeliveryRequest request) =>
        request.WicketType switch
        {
            WicketType.Bowled => $"b {request.BowlerId}",
            WicketType.Caught => $"c {request.FielderId} b {request.BowlerId}",
            WicketType.RunOut => $"run out ({request.FielderId})",
            WicketType.LBW => $"lbw b {request.BowlerId}",
            WicketType.Stumped => $"st {request.FielderId} b {request.BowlerId}",
            WicketType.HitWicket => $"hit wicket b {request.BowlerId}",
            _ => request.WicketType.ToString()
        };

    private static int CalculateBallInOver(Innings innings)
    {
        return innings.TotalLegalBalls % 6 + 1;
    }

    private static string FormatOver(int totalLegalBalls)
    {
        return $"{totalLegalBalls / 6}.{totalLegalBalls % 6}";
    }

    private async Task HandleInningsCompletionAsync(Match match, Innings completedInnings)
    {
        var allInnings = (await inningsRepository.GetByMatchAsync(match.Id)).ToList();

        if (completedInnings.InningsNumber == 1)
        {
            var battingSecondTeamId = completedInnings.BattingTeamId == match.Team1Id
                ? match.Team2Id
                : match.Team1Id;

            var secondInnings = new Innings
            {
                MatchId = match.Id,
                BattingTeamId = battingSecondTeamId,
                BowlingTeamId = completedInnings.BattingTeamId,
                InningsNumber = 2
            };

            var created = await inningsRepository.CreateAsync(secondInnings);
            match.CurrentInningsId = created.Id;
        }
        else
        {
            match.Status = MatchStatus.Completed;
            await notificationService.PublishMatchEventAsync(match.Id, "MatchCompleted", new
            {
                matchId = match.Id
            });
        }
    }

    private async Task<ScorecardResponse> BuildScorecardAsync(string matchId)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} not found.");

        var allInnings = (await inningsRepository.GetByMatchAsync(matchId))
            .OrderBy(i => i.InningsNumber)
            .ToList();

        double? rrr = null;
        int? target = null;

        if (allInnings.Count == 2)
        {
            var firstInnings = allInnings[0];
            var secondInnings = allInnings[1];

            if (!secondInnings.IsCompleted)
            {
                target = firstInnings.TotalRuns + 1;
                var runsNeeded = target.Value - secondInnings.TotalRuns;
                var ballsLeft = match.OversPerInnings * 6 - secondInnings.TotalLegalBalls;
                rrr = ballsLeft > 0 ? Math.Round(runsNeeded * 6.0 / ballsLeft, 2) : 0;
            }
        }

        var inningsDtos = allInnings.Select(inn => new InningsScorecardDto(
            inn.Id,
            inn.BattingTeamId,
            inn.InningsNumber,
            inn.TotalRuns,
            inn.Wickets,
            FormatOver(inn.TotalLegalBalls),
            inn.Extras,
            inn.TotalLegalBalls > 0 ? Math.Round(inn.TotalRuns * 6.0 / inn.TotalLegalBalls, 2) : 0,
            inn.IsCompleted,
            inn.BattingScores.Where(b => b.HasBatted || b.Balls > 0).Select(b => new BatsmanScoreDto(
                b.PlayerId, b.PlayerName, b.Runs, b.Balls,
                b.Fours, b.Sixes,
                b.Balls > 0 ? Math.Round(b.Runs * 100.0 / b.Balls, 2) : 0,
                b.IsOut, b.DismissalDescription, b.IsOnStrike
            )).ToList(),
            inn.BowlingScores.Select(b => new BowlerScoreDto(
                b.PlayerId, b.PlayerName,
                $"{b.Overs}.{b.BallsInCurrentOver}",
                b.Maidens, b.Runs, b.Wickets,
                (b.Overs * 6 + b.BallsInCurrentOver) > 0
                    ? Math.Round(b.Runs * 6.0 / (b.Overs * 6 + b.BallsInCurrentOver), 2)
                    : 0,
                b.Wides, b.NoBalls
            )).ToList(),
            inn.FallOfWickets.Select(f => new FallOfWicketDto(
                f.WicketNumber, f.PlayerName, f.RunsAtFall, f.Over
            )).ToList()
        )).ToList();

        return new ScorecardResponse(matchId, rrr, target, inningsDtos);
    }
}
