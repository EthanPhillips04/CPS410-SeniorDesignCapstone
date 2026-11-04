
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SportsAnalytics.Web.Data;
using SportsAnalytics.Web.Models;

namespace SportsAnalytics.Web.Services;

public class NbaImportService
{
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<SportsDbContext> _dbFactory;

    public NbaImportService(
        HttpClient httpClient,
        IDbContextFactory<SportsDbContext> dbFactory)
    {
        _httpClient = httpClient;
        _dbFactory = dbFactory;
    }

    // Test whether the API connection is working
    public async Task<string> TestNbaApiAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                "v1/players?per_page=10");

            response.EnsureSuccessStatusCode();

            using var stream =
                await response.Content.ReadAsStreamAsync();

            using var document =
                await JsonDocument.ParseAsync(stream);

            int count = document.RootElement
                .GetProperty("data")
                .GetArrayLength();

            return $"SUCCESS! Retrieved {count} players.";
        }
        catch (Exception ex)
        {
            return $"API ERROR: {ex.Message}";
        }
    }

    // Import the first 100 player records into PostgreSQL
    public async Task<int> ImportPlayersAsync()
    {
        using var response = await _httpClient.GetAsync(
            "v1/players?per_page=100");

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync();

        using var document =
            await JsonDocument.ParseAsync(stream);

        var playerData = document.RootElement
            .GetProperty("data");

        await using var db =
            await _dbFactory.CreateDbContextAsync();

        // Get existing records to prevent duplicates
        var existingPlayers = await db.NbaPlayers
            .ToDictionaryAsync(p => p.BallDontLiePlayerId);

        var existingTeams = await db.NbaTeams
            .ToDictionaryAsync(t => t.BallDontLieTeamId);

        int importedCount = 0;

        foreach (var item in playerData.EnumerateArray())
        {
            int playerId = item.GetProperty("id").GetInt32();

            string firstName = GetString(item, "first_name");
            string lastName = GetString(item, "last_name");
            string position = GetString(item, "position");

            NbaTeam? team = null;

            if (item.TryGetProperty("team", out var teamJson)
                && teamJson.ValueKind == JsonValueKind.Object
                && teamJson.TryGetProperty("id", out var idJson)
                && idJson.ValueKind == JsonValueKind.Number)
            {
                int teamId = idJson.GetInt32();

                if (teamId > 0)
                {
                    if (!existingTeams.TryGetValue(teamId, out team))
                    {
                        team = new NbaTeam
                        {
                            BallDontLieTeamId = teamId
                        };

                        db.NbaTeams.Add(team);
                        existingTeams[teamId] = team;
                    }

                    team.Name = GetString(teamJson, "full_name");
                    team.Abbreviation =
                        GetString(teamJson, "abbreviation");
                    team.Conference =
                        GetString(teamJson, "conference");
                }
            }

            if (!existingPlayers.TryGetValue(playerId, out var player))
            {
                player = new NbaPlayer
                {
                    BallDontLiePlayerId = playerId
                };

                db.NbaPlayers.Add(player);
                existingPlayers[playerId] = player;
            }

            player.FirstName = firstName;
            player.LastName = lastName;
            player.Position = position;
            player.Team = team;

            importedCount++;
        }

        await db.SaveChangesAsync();

        return importedCount;
    }

    private static string GetString(
        JsonElement item, string propertyName)
    {
        if (item.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? "";
        }

        return "";
    }
}
