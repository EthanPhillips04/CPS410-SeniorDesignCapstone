using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SportsAnalytics.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNbaGamesAndPlayerStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "NbaPlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRosterSyncUtc",
                table: "NbaPlayers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NbaGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BallDontLieGameId = table.Column<int>(type: "integer", nullable: false),
                    GameDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Season = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsFinal = table.Column<bool>(type: "boolean", nullable: false),
                    HomeTeamId = table.Column<int>(type: "integer", nullable: false),
                    VisitorTeamId = table.Column<int>(type: "integer", nullable: false),
                    HomeTeamScore = table.Column<int>(type: "integer", nullable: false),
                    VisitorTeamScore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NbaGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NbaPlayerGameStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NbaPlayerId = table.Column<int>(type: "integer", nullable: false),
                    NbaGameId = table.Column<int>(type: "integer", nullable: false),
                    BallDontLieTeamId = table.Column<int>(type: "integer", nullable: false),
                    Minutes = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: true),
                    Rebounds = table.Column<int>(type: "integer", nullable: true),
                    Assists = table.Column<int>(type: "integer", nullable: true),
                    Steals = table.Column<int>(type: "integer", nullable: true),
                    Blocks = table.Column<int>(type: "integer", nullable: true),
                    Turnovers = table.Column<int>(type: "integer", nullable: true),
                    OffensiveRebounds = table.Column<int>(type: "integer", nullable: true),
                    DefensiveRebounds = table.Column<int>(type: "integer", nullable: true),
                    FieldGoalsMade = table.Column<int>(type: "integer", nullable: true),
                    FieldGoalsAttempted = table.Column<int>(type: "integer", nullable: true),
                    ThreePointersMade = table.Column<int>(type: "integer", nullable: true),
                    ThreePointersAttempted = table.Column<int>(type: "integer", nullable: true),
                    FreeThrowsMade = table.Column<int>(type: "integer", nullable: true),
                    FreeThrowsAttempted = table.Column<int>(type: "integer", nullable: true),
                    PersonalFouls = table.Column<int>(type: "integer", nullable: true),
                    PlusMinus = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NbaPlayerGameStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NbaPlayerGameStats_NbaGames_NbaGameId",
                        column: x => x.NbaGameId,
                        principalTable: "NbaGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NbaPlayerGameStats_NbaPlayers_NbaPlayerId",
                        column: x => x.NbaPlayerId,
                        principalTable: "NbaPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NbaGames_BallDontLieGameId",
                table: "NbaGames",
                column: "BallDontLieGameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NbaPlayerGameStats_NbaGameId",
                table: "NbaPlayerGameStats",
                column: "NbaGameId");

            migrationBuilder.CreateIndex(
                name: "IX_NbaPlayerGameStats_NbaPlayerId_NbaGameId",
                table: "NbaPlayerGameStats",
                columns: new[] { "NbaPlayerId", "NbaGameId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NbaPlayerGameStats");

            migrationBuilder.DropTable(
                name: "NbaGames");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "NbaPlayers");

            migrationBuilder.DropColumn(
                name: "LastRosterSyncUtc",
                table: "NbaPlayers");
        }
    }
}
