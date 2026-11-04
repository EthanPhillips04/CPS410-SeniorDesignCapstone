
namespace SportsAnalytics.Web.Models;

public class NbaTeam
{
    public int Id { get; set; }

    public int BallDontLieTeamId { get; set; }

    public string Name { get; set; } = "";

    public string Abbreviation { get; set; } = "";

    public string Conference { get; set; } = "";

    public List<NbaPlayer> Players { get; set; } = new();
}
