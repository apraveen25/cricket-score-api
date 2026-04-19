namespace CricketScore.Application.DTOs.Scoring;

public record BatsmanScoreDto(
    string PlayerId,
    string PlayerName,
    int Runs,
    int Balls,
    int Fours,
    int Sixes,
    double StrikeRate,
    bool IsOut,
    string? DismissalDescription,
    bool IsOnStrike
);

public record BowlerScoreDto(
    string PlayerId,
    string PlayerName,
    string Overs,
    int Maidens,
    int Runs,
    int Wickets,
    double Economy,
    int Wides,
    int NoBalls
);

public record FallOfWicketDto(
    int WicketNumber,
    string PlayerName,
    int RunsAtFall,
    string Over
);

public record InningsScorecardDto(
    string InningsId,
    string BattingTeamId,
    int InningsNumber,
    int TotalRuns,
    int Wickets,
    string Overs,
    int Extras,
    double RunRate,
    bool IsCompleted,
    List<BatsmanScoreDto> Batting,
    List<BowlerScoreDto> Bowling,
    List<FallOfWicketDto> FallOfWickets
);

public record ScorecardResponse(
    string MatchId,
    double? RequiredRunRate,
    int? Target,
    List<InningsScorecardDto> Innings
);
