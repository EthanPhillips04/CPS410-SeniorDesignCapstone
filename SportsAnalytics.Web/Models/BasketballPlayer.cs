namespace SportsAnalytics.Web.Models;

public class BasketballPlayer
{
    public int Id { get; set; }

    public int NbaPlayerId { get; set; }

    public string Season { get; set; } = "";

    public string Name { get; set; } = "";

    public string Team { get; set; } = "";

    public int GamesPlayed { get; set; }

    public decimal Points { get; set; }

    public decimal Rebounds { get; set; }

    public decimal Assists { get; set; }
}

