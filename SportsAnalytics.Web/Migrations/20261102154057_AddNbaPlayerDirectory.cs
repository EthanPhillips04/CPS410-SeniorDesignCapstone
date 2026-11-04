using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SportsAnalytics.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNbaPlayerDirectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NbaTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BallDontLieTeamId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Abbreviation = table.Column<string>(type: "text", nullable: false),
                    Conference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NbaTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NbaPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BallDontLiePlayerId = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    NbaTeamId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NbaPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NbaPlayers_NbaTeams_NbaTeamId",
                        column: x => x.NbaTeamId,
                        principalTable: "NbaTeams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NbaPlayers_BallDontLiePlayerId",
                table: "NbaPlayers",
                column: "BallDontLiePlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NbaPlayers_NbaTeamId",
                table: "NbaPlayers",
                column: "NbaTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_NbaTeams_BallDontLieTeamId",
                table: "NbaTeams",
                column: "BallDontLieTeamId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NbaPlayers");

            migrationBuilder.DropTable(
                name: "NbaTeams");
        }
    }
}
