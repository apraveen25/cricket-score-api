using CricketScore.Domain.Enums;

namespace CricketScore.Application.DTOs.Scoring;

public record LiveBatsmanDto(
    string PlayerId,
    string PlayerName,
    bool IsOnStrike,
    int Runs,
    int Balls,
    int Fours,
    int Sixes,
    double StrikeRate
);

public record LiveBowlerDto(
    string PlayerId,
    string PlayerName,
    string Overs,
    int Maidens,
    int Runs,
    int Wickets,
    double Economy
);

public record CurrentOverBallDto(
    int SequenceIndex,
    string Display,
    int RunsScored,
    bool IsWicket,
    bool IsExtra,
    ExtraType ExtraType
);

public record LiveScoringStateResponse(
    string MatchId,
    string InningsId,
    string BattingTeamId,
    string BattingTeamName,
    string BowlingTeamId,
    string BowlingTeamName,
    int InningsNumber,
    int TotalRuns,
    int Wickets,
    string Overs,
    int? Target,
    int? RunsNeeded,
    int? BallsRemaining,
    double CurrentRunRate,
    double? RequiredRunRate,
    LiveBatsmanDto? StrikeBatsman,
    LiveBatsmanDto? NonStrikeBatsman,
    LiveBowlerDto? CurrentBowler,
    List<CurrentOverBallDto> CurrentOverBalls,
    int RunsInCurrentOver,
    int TotalOvers,
    bool IsCompleted
);

public record OverRunRateDto(
    int OverNumber,
    int RunsInOver,
    int WicketsInOver,
    double CumulativeRunRate,
    int CumulativeRuns,
    int CumulativeWickets
);

public record RunRateChartResponse(
    string MatchId,
    string InningsId,
    int TotalOvers,
    List<OverRunRateDto> Overs
);

public record WagonWheelShotDto(
    double Angle,
    double Distance,
    int Runs,
    bool IsWicket,
    bool IsExtra
);

public record WagonWheelResponse(
    string MatchId,
    string InningsId,
    List<WagonWheelShotDto> Shots
);
