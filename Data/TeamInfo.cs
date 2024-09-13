using System.ComponentModel.DataAnnotations;

namespace MLB_Database.Data
{
    public class TeamInfo 
    {
        [Key]
        public int Id { get; set; }
        public int MLBId { get; set; }
        public string Team_League_Logo { get; set; }
        public string Team_League { get; set; }
        public string Team_Division { get; set; }
        public string Team_Abreviation { get; set; }
        public string Team_Name { get; set; }
        public string Team_Logo { get; set; }
        public int Team_Established { get; set; }
        public string Team_Ballpark { get; set; }
        public string Team_URL { get; set; }
        public int Team_Standing { get; set; }
        public int Team_Wins { get; set; }
        public int Team_Losses { get; set; }
        public int StadiumId { get; set; }
    }
}
