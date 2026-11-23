
namespace SportsAnalytics.Web.Models;

public class NbaGame
{
    public int Id { get; set; }

    // ID supplied by BALLDONTLIE
    public int BallDontLieGameId { get; set; }

    public DateOnly GameDate { get; set; }

    public int Season { get; set; }

    public string Status { get; set; } = "";

    public bool IsFinal { get; set; } = false;

    // BALLDONTLIE team IDs
    public int HomeTeamId { get; set; }

    public int VisitorTeamId { get; set; }

    public int HomeTeamScore { get; set; }

    public int VisitorTeamScore { get; set; }
}
