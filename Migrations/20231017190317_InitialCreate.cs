using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MLB_Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MLBId = table.Column<int>(type: "int", nullable: false),
                    Team_League_Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_League = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Division = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Abreviation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Established = table.Column<int>(type: "int", nullable: false),
                    Team_Ballpark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Team_Standing = table.Column<int>(type: "int", nullable: false),
                    Team_Wins = table.Column<int>(type: "int", nullable: false),
                    Team_Losses = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInfo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamInfo");
        }
    }
}
