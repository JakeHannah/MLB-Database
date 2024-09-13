using MLB_Database.Shared;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;





public class MlbApiService
{
    private readonly HttpClient _httpClient;

    public MlbApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string GetPlayerImageUrl(int playerId)
    {
        return $"https://img.mlbstatic.com/mlb-photos/image/upload/d_people:generic:headshot:silo:current.png/r_max/w_180,q_auto:best/v1/people/{playerId}/headshot/silo/current";
    }

    public async Task<LinescoreResponse> GetCurrentGameStats(int gamePk)
    {
        var response = await _httpClient.GetFromJsonAsync<LinescoreResponse>($"https://statsapi.mlb.com/api/v1/game/{gamePk}/linescore");
        return response;
    }


    public async Task<MlbLeadersStatsResponse> GetLeaderStats(string group, string leagueId, int year)
{
    var response = await _httpClient.GetFromJsonAsync<MlbLeadersStatsResponse>($"https://statsapi.mlb.com/api/v1/stats?group={group}&stats=season&leagueId={leagueId}&season={year}&gameType=R&limit=5&offset=0");
    return response;
}

    public async Task<MlbHRLeadersStatsResponse> GetHRLeaderStats(string leagueId, int year)
    {
        var response = await _httpClient.GetFromJsonAsync<MlbHRLeadersStatsResponse>($"https://statsapi.mlb.com/api/v1/stats/leaders?leaderCategories=homeRuns&hydrate=person,team&limit=15&sortColumn=homeRuns&sortOrder=desc&season={year}&sportId=1");
        return response;
    }

    public async Task<MlbPlayerStatsResponse> GetPlayerSeasonStats(int playerId, int year, string group)
    {
        var response = await _httpClient.GetFromJsonAsync<MlbPlayerStatsResponse>($"https://statsapi.mlb.com/api/v1/people/{playerId}/stats?stats=season&season={year}&gameType=R&group={group}");

        return response;
    }

    public async Task<MatchUpResponse> GetMatchUp(int teamId, DateTime date = default)
    {
        if (date == default) // if date is not provided, use today's date
        {
            date = DateTime.Today;
        }

        var response = await _httpClient.GetFromJsonAsync<MatchUpResponse>($"https://statsapi.mlb.com/api/v1/schedule?sportId=1&teamId={teamId}&startDate={date:yyyy-MM-dd}&endDate={date:yyyy-MM-dd}&language=en&hydrate=linescore,boxscore");
        return response;
    }
    public async Task<MlbRosterResponse> GetTeamRoster(int teamId)
    {
        var response = await _httpClient.GetFromJsonAsync<MlbRosterResponse>($"https://statsapi.mlb.com/api/v1/teams/{teamId}/roster");

        return response;
    }


    public async Task<TeamRecord> GetTeamRecord(int teamId, int year)
    {
        var response = await _httpClient.GetFromJsonAsync<MlbStandingsResponse>($"https://statsapi.mlb.com/api/v1/standings?leagueId=103,104&season={year}&standingsTypes=regularSeason");




        var teamRecord = response.Records.SelectMany(record => record.TeamRecords).FirstOrDefault(tr => tr.Team.Id == teamId);




        if (teamRecord == null)
        {
            return new TeamRecord { };
        }

        return teamRecord;
    }

    public async Task<MatchUpResponse> GetTeamSchedule(int teamId, DateTime startDate, DateTime endDate)
    {
        var response = await _httpClient.GetFromJsonAsync<MatchUpResponse>($"https://statsapi.mlb.com/api/v1/schedule?sportId=1&teamId={teamId}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&language=en&hydrate=linescore,boxscore");
        return response;
    }

    public async Task<AllStarFinalVoteResponse> GetAllStarFinalVote(string leagueId, int season, string fields = null)
    {
        var url = $"https://statsapi.mlb.com/api/v1/league/{leagueId}/allStarFinalVote?season={season}";
        if (!string.IsNullOrEmpty(fields))
        {
            url += $"&fields={fields}";
        }

        var response = await _httpClient.GetFromJsonAsync<AllStarFinalVoteResponse>(url);
        return response;
    }
}

public class LinescoreResponse
{
    public TeamsLinescore Teams { get; set; }
    public Innings[] Innings { get; set; }
    public int CurrentInning { get; set; }
    public string CurrentInningOrdinal { get; set; }
    public string InningState { get; set; }
    public int Outs { get; set; }
    public int Balls { get; set; }
    public int Strikes { get; set; }
    public bool IsTopInning { get; set; }
}

public class TeamsLinescore
{
    public TeamLinescore Home { get; set; }
    public TeamLinescore Away { get; set; }
}

public class TeamLinescore
{
    public string Name { get; set; }
    public int Runs { get; set; }
    public int Hits { get; set; }
    public int Errors { get; set; }
}

public class Innings
{
    public int Num { get; set; }
    public string OrdinalNum { get; set; }
    public InningLine Home { get; set; }
    public InningLine Away { get; set; }
}

public class InningLine
{
    public int Runs { get; set; }
    public int Hits { get; set; }
    public int Errors { get; set; }
    public int LeftOnBase { get; set; }
}



public class MatchUpResponse
{
    public int TotalGames { get; set; }
    public Dates[] Dates { get; set; }
}

public class MlbLeadersStatsResponse
{
    public Stats[] Stats { get; set; }
}

public class MlbHRLeadersStatsResponse
{
    public LeagueLeaders[] LeagueLeaders { get; set; } 
}
public class LeagueLeaders
{
    public Leaders[] Leaders { get; set; }
}
public class Leaders
{
    public int Rank { get; set; }
    public Person Person { get; set; }
    public string Value { get; set; }
    public Team Team { get; set; }
}

public class Dates
{
    public string Date { get; set; }
    public Games[] Games { get; set; }
}

public class Games
{
    public string Link { get; set; }
    public string GameType { get; set; }
    public Teams Teams { get; set; }
    public string GameDate { get; set; }
    public string OfficialDate {get; set;}
    public int GamePk { get; set; }

}

public class Teams
{
    public Home Home { get; set; }
    public Away Away { get; set; }
}

public class Home
{
    public LeagueRecord LeagueRecord {  get; set; }
    public Team Team { get; set; }
}

public class Away
{
    public LeagueRecord LeagueRecord { get; set; }
    public Team Team { get; set; }
}

public class LeagueRecord
{
    public int Wins {get; set; }
    public int Losses { get; set; }
}
public class Type
{
    public string DisplayName { get; set; }
}
public class Group
{
    public string DisplayName { get; set; }
}

public class MlbPlayerStatsResponse
{
    public Stats[] Stats { get; set; }
}

public class Stats
{
    public Splits[] Splits { get; set; }
    public Group Group { get; set; }
}
public class Splits
{
    public Stat Stat { get; set; }
    public Team Team { get; set; }
    public Player Player { get; set; }
}
public class Stat
{
    public int gamesPlayed { get; set; }
    public int groundOuts { get; set; }
    public int airOuts { get; set; }
    public int runs { get; set; }
    public int doubles { get; set; }
    public int triples { get; set; }
    public int homeRuns { get; set; }
    public int strikeOuts { get; set; }
    public int baseOnBalls { get; set; }
    public int intentionalWalks { get; set; }
    public int hits { get; set; }
    public int hitByPitch { get; set; }
    public string avg { get; set; }
    public int atBats { get; set; }
    public string obp { get; set; }
    public string slg { get; set; }
    public string ops { get; set; }
    public int caughtStealing { get; set; }
    public int stolenBases { get; set; }
    public string stolenBasePercentage { get; set; }
    public int groundIntoDoublePlay { get; set; }
    public int numberOfPitches { get; set; }
    public int plateAppearances { get; set; }
    public int totalBases { get; set; }
    public int rbi { get; set; }
    public int leftOnBase { get; set; }
    public int sacBunts { get; set; }
    public int sacFlies { get; set; }
    public string babip { get; set; }
    public string groundOutsToAirouts { get; set; }
    public int catchersInterference { get; set; }
    public string atBatsPerHomeRun { get; set; }
    public int gamesStarted { get; set; }
    public string era { get; set; }
    public string inningsPitched { get; set; }
    public int wins { get; set; }
    public int losses { get; set; }
    public int saves { get; set; }
    public int saveOpportunities { get; set; }
    public int holds { get; set; }
    public int blownSaves { get; set; }
    public int earnedRuns { get; set; }
    public string whip { get; set; }
    public int battersFaced { get; set; }
    public int outs { get; set; }
    public int gamesPitched { get; set; }
    public int completeGames { get; set; }
    public int shutouts { get; set; }
    public int strikes { get; set; }
    public string strikePercentage { get; set; }
    public int hitBatsmen { get; set; }
    public int balks { get; set; }
    public int wildPitches { get; set; }
    public int pickoffs { get; set; }
    public string winPercentage { get; set; }
    public string pitchesPerInning { get; set; }
    public int gamesFinished { get; set; }
    public string strikeoutWalkRatio { get; set; }
    public string strikeoutsPer9Inn { get; set; }
    public string walksPer9Inn { get; set; }
    public string hitsPer9Inn { get; set; }
    public string runsScoredPer9 { get; set; }
    public string homeRunsPer9 { get; set; }
    public int inheritedRunners { get; set; }
    public int inheritedRunnersScored { get; set; }
}
public class StatGroup
{
    public string DisplayName { get; set; }
}

public class StatType
{
    public string DisplayName { get; set; }
}

public class PlayerStats
{
    public int HomeRuns { get; set; }
    public int RunsBattedIn { get; set; }
    public double BattingAverage { get; set; }
}

public class MlbStandingsResponse
{
    public StandingsRecord[] Records { get; set; }
}




public class StandingsRecord
{
    public TeamRecord[] TeamRecords { get; set; }
}




public class TeamRecord
{
    public Team Team { get; set; }
    public LeagueRecord LeagueRecord { get; set; }
    public string DivisionRank { get; set; }
    public Streak Streak { get; set; }
    public string GamesBack { get; set; }
    public Records Records { get; set; } 
}

public class Records
{
    public bool Clinched { get; set; }
    public bool HasWildCard { get; set; }
}

public class Streak
{
    public string StreakCode { get; set; }
}

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class MlbRosterResponse
{
    public Player[] Roster { get; set; }
}

public class Player
{
    public Person Person { get; set; }
    public Position Position { get; set; }
    public string JerseyNumber { get; set; }
    public int Id { get; set; }
    public string FullName { get; set; }
    public string LastName { get; set; }
}
public class Person
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Image { get; set; }
    public string BirthCountry { get; set; }
    public MlbPlayerStatsResponse Stats { get; set; }
}
public class Position
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Abbreviation { get; set; }

}

// AllStarFinalVote
public class AllStarFinalVoteResponse {
    public string LeagueId { get; set; }
    public string Season { get; set; }
    public List<FinalVoteCandidate> Candidates { get; set; }
}

public class FinalVoteCandidate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Team { get; set; }
    public int Votes { get; set; }
}
