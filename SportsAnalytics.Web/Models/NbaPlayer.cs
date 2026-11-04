
namespace SportsAnalytics.Web.Models;

public class NbaPlayer
{
    public int Id { get; set; }

    public int BallDontLiePlayerId { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Position { get; set; } = "";

    public int? NbaTeamId { get; set; }

    public NbaTeam? Team { get; set; }
}
