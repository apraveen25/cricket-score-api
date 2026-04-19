using AutoMapper;
using CricketScore.Application.DTOs.Matches;
using CricketScore.Domain.Entities;
using CricketScore.Domain.Enums;
using CricketScore.Domain.Interfaces.Repositories;
using CricketScore.Domain.Interfaces.Services;

namespace CricketScore.Application.Services;

public class MatchService(
    IMatchRepository matchRepository,
    ITeamRepository teamRepository,
    IInningsRepository inningsRepository,
    INotificationService notificationService,
    IMapper mapper)
{
    public async Task<MatchResponse> CreateMatchAsync(CreateMatchRequest request, string userId)
    {
        var team1 = await teamRepository.GetByIdAsync(request.Team1Id)
            ?? throw new KeyNotFoundException($"Team {request.Team1Id} not found.");
        var team2 = await teamRepository.GetByIdAsync(request.Team2Id)
            ?? throw new KeyNotFoundException($"Team {request.Team2Id} not found.");

        var match = new Match
        {
            Team1Id = team1.Id,
            Team2Id = team2.Id,
            Format = request.Format,
            OversPerInnings = request.OversPerInnings,
            ScheduledAt = request.ScheduledAt,
            Venue = request.Venue,
            CreatedBy = userId,
            PlayingXIs = request.PlayingXIs.Select(xi => new PlayingXI
            {
                TeamId = xi.TeamId,
                PlayerIds = xi.PlayerIds
            }).ToList()
        };

        var created = await matchRepository.CreateAsync(match);
        return mapper.Map<MatchResponse>(created);
    }

    public async Task<MatchResponse> GetMatchAsync(string id)
    {
        var match = await matchRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Match {id} not found.");

        return mapper.Map<MatchResponse>(match);
    }

    public async Task<MatchResponse> StartMatchAsync(string matchId, StartMatchRequest request, string userId)
    {
        var match = await matchRepository.GetByIdAsync(matchId)
            ?? throw new KeyNotFoundException($"Match {matchId} not found.");

        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Match can only be started from Scheduled state.");

        if (match.CreatedBy != userId)
            throw new UnauthorizedAccessException("Only the match creator can start the match.");

        if (match.Team1Id != request.TossWinnerTeamId && match.Team2Id != request.TossWinnerTeamId)
            throw new ArgumentException("Toss winner must be one of the match teams.");

        match.Toss = new TossResult
        {
            WinnerTeamId = request.TossWinnerTeamId,
            Decision = request.TossDecision
        };

        match.BattingFirstTeamId = request.TossDecision == TossDecision.Bat
            ? request.TossWinnerTeamId
            : (match.Team1Id == request.TossWinnerTeamId ? match.Team2Id : match.Team1Id);

        var bowlingTeamId = match.BattingFirstTeamId == match.Team1Id ? match.Team2Id : match.Team1Id;

        var innings = new Innings
        {
            MatchId = match.Id,
            BattingTeamId = match.BattingFirstTeamId,
            BowlingTeamId = bowlingTeamId,
            InningsNumber = 1
        };

        var createdInnings = await inningsRepository.CreateAsync(innings);
        match.CurrentInningsId = createdInnings.Id;
        match.Status = MatchStatus.Live;

        var updated = await matchRepository.UpdateAsync(match);

        await notificationService.PublishMatchEventAsync(match.Id, "MatchStarted", new
        {
            matchId = match.Id,
            battingFirstTeamId = match.BattingFirstTeamId
        });

        return mapper.Map<MatchResponse>(updated);
    }

    public async Task<IEnumerable<MatchResponse>> GetRecentMatchesAsync()
    {
        var matches = await matchRepository.GetRecentAsync();
        return mapper.Map<IEnumerable<MatchResponse>>(matches);
    }
}
