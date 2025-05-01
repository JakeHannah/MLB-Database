using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class PlayByPlayService
{
    private readonly HttpClient _httpClient;

    public PlayByPlayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PlayByPlayResponse> GetPlayByPlayAsync(int gamePk)
    {
        return await _httpClient
            .GetFromJsonAsync<PlayByPlayResponse>(
                $"https://statsapi.mlb.com/api/v1/game/{gamePk}/playByPlay"
            );
    }
}

// Top‐level response
public class PlayByPlayResponse
{
    public string Copyright { get; set; }
    public AllPlay[] AllPlays { get; set; }
}

// One entry per play
public class AllPlay
{
    public Result Result { get; set; }
    public About About { get; set; }
    public Count Count { get; set; }
    public Matchup Matchup { get; set; }
    public int[] PitchIndex { get; set; }
    public int[] ActionIndex { get; set; }
    public int[] RunnerIndex { get; set; }
    public Runner[] Runners { get; set; }
    public PlayEvent[] PlayEvents { get; set; }
}

public class Result
{
    public string Type { get; set; }
    public string Event { get; set; }
    public string EventType { get; set; }
    public string Description { get; set; }
    public int Rbi { get; set; }
    public int AwayScore { get; set; }
    public int HomeScore { get; set; }
    public bool IsOut { get; set; }
}

public class About
{
    public int AtBatIndex { get; set; }
    public string HalfInning { get; set; }
    public bool IsTopInning { get; set; }
    public int Inning { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsComplete { get; set; }
    public bool IsScoringPlay { get; set; }
    public bool HasReview { get; set; }
    public bool HasOut { get; set; }
    public int CaptivatingIndex { get; set; }
}

public class Count
{
    public int Balls { get; set; }
    public int Strikes { get; set; }
    public int Outs { get; set; }
}

public class Matchup
{
    public PersonRef Batter { get; set; }
    public BatSide BatSide { get; set; }
    public PersonRef Pitcher { get; set; }
    public PitchHand PitchHand { get; set; }
    public object[] BatterHotColdZones { get; set; }
    public object[] PitcherHotColdZones { get; set; }
    public Splits Splits { get; set; }
}

public class PersonRef
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Link { get; set; }
}

public class BatSide
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class PitchHand
{
    public string Code { get; set; }
    public string Description { get; set; }
}


public class Runner
{
    public Movement Movement { get; set; }
    public RunnerDetails Details { get; set; }
    public Credit[] Credits { get; set; }
}

public class Movement
{
    public string OriginBase { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public string OutBase { get; set; }
    public bool? IsOut { get; set; }
    public int? OutNumber { get; set; }
}

public class RunnerDetails
{
    public string Event { get; set; }
    public string EventType { get; set; }
    public string Description { get; set; }
    public string MovementReason { get; set; }
    public PersonRef Runner { get; set; }
    public PersonRef ResponsiblePitcher { get; set; }
    public bool IsScoringEvent { get; set; }
    public bool Rbi { get; set; }
    public bool Earned { get; set; }
    public bool TeamUnearned { get; set; }
    public int PlayIndex { get; set; }
}

public class Credit
{
    public PersonRef Player { get; set; }
    public PositionRef Position { get; set; }
    //public string Credit { get; set; }
}

public class PositionRef
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Abbreviation { get; set; }
}

public class PlayEvent
{
    public PlayEventDetails Details { get; set; }
    public Count Count { get; set; }
    public int Index { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsPitch { get; set; }
    public string Type { get; set; }
    public PersonRef Player { get; set; }
    public PitchData PitchData { get; set; }
    public int PitchNumber { get; set; }
    public string PlayId { get; set; }
}

public class PlayEventDetails
{
    // common fields
    public string Description { get; set; }
    public string Event { get; set; }
    public string EventType { get; set; }
    public int AwayScore { get; set; }
    public int HomeScore { get; set; }
    public bool IsScoringPlay { get; set; }
    public bool IsOut { get; set; }
    public bool HasReview { get; set; }

    // pitch‐specific
    public Call Call { get; set; }
    public string Code { get; set; }
    public string BallColor { get; set; }
    public string TrailColor { get; set; }
    public bool IsInPlay { get; set; }
    public bool IsStrike { get; set; }
    public bool IsBall { get; set; }
    public PitchType Type { get; set; }
}

public class Call
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class PitchType
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class PitchData
{
    public double StartSpeed { get; set; }
    public double EndSpeed { get; set; }
    public double StrikeZoneTop { get; set; }
    public double StrikeZoneBottom { get; set; }
    public Coordinates Coordinates { get; set; }
    public Breaks Breaks { get; set; }
    public int Zone { get; set; }
    public double TypeConfidence { get; set; }
    public double PlateTime { get; set; }
    public double Extension { get; set; }
}

public class Coordinates
{
    public double AY { get; set; }
    public double AZ { get; set; }
    public double PfxX { get; set; }
    public double PfxZ { get; set; }
    public double PX { get; set; }
    public double PZ { get; set; }
    public double VX0 { get; set; }
    public double VY0 { get; set; }
    public double VZ0 { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double X0 { get; set; }
    public double Y0 { get; set; }
    public double Z0 { get; set; }
    public double AX { get; set; }
}

public class Breaks
{
    public double BreakAngle { get; set; }
    public double BreakLength { get; set; }
    public double BreakY { get; set; }
    public double BreakVertical { get; set; }
    public double BreakVerticalInduced { get; set; }
    public double BreakHorizontal { get; set; }
    public int SpinRate { get; set; }
    public int SpinDirection { get; set; }
}
