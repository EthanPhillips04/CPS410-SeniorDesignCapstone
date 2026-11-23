
namespace SportsAnalytics.Web.Models;

public class NbaPlayerGameStat
{
    public int Id { get; set; }

    // Reference to the stored player
    public int NbaPlayerId { get; set; }

    public NbaPlayer Player { get; set; } = null!;

    // Reference to the stored game
    public int NbaGameId { get; set; }

    public NbaGame Game { get; set; } = null!;

    // Team represented during this game
    public int BallDontLieTeamId { get; set; }

    // Playing time
    public string Minutes { get; set; } = "";

    // Main box-score statistics
    public int? Points { get; set; }

    public int? Rebounds { get; set; }

    public int? Assists { get; set; }

    public int? Steals { get; set; }

    public int? Blocks { get; set; }

    public int? Turnovers { get; set; }

    // Rebounding statistics
    public int? OffensiveRebounds { get; set; }

    public int? DefensiveRebounds { get; set; }

    // Field goal statistics
    public int? FieldGoalsMade { get; set; }

    public int? FieldGoalsAttempted { get; set; }

    // Three-point shooting statistics
    public int? ThreePointersMade { get; set; }

    public int? ThreePointersAttempted { get; set; }

    // Free throw statistics
    public int? FreeThrowsMade { get; set; }

    public int? FreeThrowsAttempted { get; set; }

    // Additional statistics
    public int? PersonalFouls { get; set; }

    public int? PlusMinus { get; set; }
}
