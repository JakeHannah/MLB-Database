using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MLB_Database.Data;

namespace MLB_Database.Services
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
        public DbSet<TeamInfo> TeamInfo { get; set; }


        public async Task<TeamInfo> UpdateTeamInfoAsync(TeamInfo updatedTeamInfo)
        {
            var existingTeamInfo = await TeamInfo.FindAsync(updatedTeamInfo.Id);

            if (existingTeamInfo == null)
            {
                // Handle not found case, e.g., throw an exception or return null
                return null;
            }

            // Update the properties of the existing team info with new values
            existingTeamInfo.MLBId = updatedTeamInfo.MLBId;
            existingTeamInfo.Team_League_Logo = updatedTeamInfo.Team_League_Logo;
            existingTeamInfo.Team_League = updatedTeamInfo.Team_League;
            existingTeamInfo.Team_Division = updatedTeamInfo.Team_Division;
            existingTeamInfo.Team_Abreviation = updatedTeamInfo.Team_Abreviation;
            existingTeamInfo.Team_Name = updatedTeamInfo.Team_Name;
            existingTeamInfo.Team_Logo = updatedTeamInfo.Team_Logo;
            existingTeamInfo.Team_Established = updatedTeamInfo.Team_Established;
            existingTeamInfo.Team_Ballpark = updatedTeamInfo.Team_Ballpark;
            existingTeamInfo.Team_URL = updatedTeamInfo.Team_URL;
            existingTeamInfo.Team_Standing = updatedTeamInfo.Team_Standing;
            existingTeamInfo.Team_Wins = updatedTeamInfo.Team_Wins;
            existingTeamInfo.Team_Losses = updatedTeamInfo.Team_Losses;
            existingTeamInfo.StadiumId = updatedTeamInfo.StadiumId;

            await SaveChangesAsync();

            return existingTeamInfo;
        }
    }
}
