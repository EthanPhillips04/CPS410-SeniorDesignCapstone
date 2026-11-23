
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


    public async Task<int> ImportPlayersAsync()
    {
        // Download every active-player page.
        // If a request fails, no database changes occur.
        var apiPlayers = await GetAllActivePlayersAsync();

        var activeIds = new HashSet<int>();

        foreach (var item in apiPlayers)
        {
            int id = item.GetProperty("id").GetInt32();

            if (id <= 0 || !activeIds.Add(id))
            {
                throw new InvalidOperationException(
                    $"Invalid or duplicate player ID: {id}");
            }
        }

        // Open the database.
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        // Keep all roster changes together.
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var existingPlayers = await db.NbaPlayers
            .ToDictionaryAsync(p => p.BallDontLiePlayerId);

        var existingTeams = await db.NbaTeams
            .ToDictionaryAsync(t => t.BallDontLieTeamId);

        // Mark previous records inactive.
        // We only do this AFTER downloading every page.
        foreach (var player in existingPlayers.Values)
        {
            player.IsActive = false;
        }

        DateTime syncTime = DateTime.UtcNow;

        // Insert or update all active players.
        foreach (var item in apiPlayers)
        {
            int playerId =
                item.GetProperty("id").GetInt32();

            string firstName =
                GetString(item, "first_name");

            string lastName =
                GetString(item, "last_name");

            string position =
                GetString(item, "position");

            NbaTeam? team = null;

            // Read the player's current team.
            if (item.TryGetProperty("team", out var teamJson) &&
                teamJson.ValueKind == JsonValueKind.Object &&
                teamJson.TryGetProperty("id", out var teamIdJson) &&
                teamIdJson.ValueKind == JsonValueKind.Number)
            {
                int teamId = teamIdJson.GetInt32();

                if (teamId > 0)
                {
                    // Reuse a team if it is already stored.
                    if (!existingTeams.TryGetValue(
                        teamId, out team))
                    {
                        team = new NbaTeam
                        {
                            BallDontLieTeamId = teamId
                        };

                        db.NbaTeams.Add(team);

                        existingTeams[teamId] = team;
                    }

                    team.Name =
                        GetString(teamJson, "full_name");

                    team.Abbreviation =
                        GetString(teamJson, "abbreviation");

                    team.Conference =
                        GetString(teamJson, "conference");
                }
            }

            // Reuse an existing player or create a new one.
            if (!existingPlayers.TryGetValue(
                playerId, out var player))
            {
                player = new NbaPlayer
                {
                    BallDontLiePlayerId = playerId
                };

                db.NbaPlayers.Add(player);

                existingPlayers[playerId] = player;
            }

            // Update the player information.
            player.FirstName = firstName;
            player.LastName = lastName;
            player.Position = position;

            player.Team = team;

            player.IsActive = true;
            player.LastRosterSyncUtc = syncTime;
        }

        await db.SaveChangesAsync();

        // Commit only after everything succeeds.
        await transaction.CommitAsync();

        return activeIds.Count;
    }

    //Import all game stats for players for a game date - code from BallDontLie Fourms page on how to import a days stats.
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

    public async Task<string> TestAllStarApiAsync()
    {
        try
        {
            // Test 1: Active players
            using var playersResponse =
                await _httpClient.GetAsync(
                    "v1/players/active?per_page=10");

            if (!playersResponse.IsSuccessStatusCode)
            {
                return $"Active Players API failed: " +
                       $"{(int)playersResponse.StatusCode} " +
                       $"{playersResponse.ReasonPhrase}";
            }

            using var playersJson =
                JsonDocument.Parse(
                    await playersResponse.Content.ReadAsStringAsync());

            int playerCount = playersJson.RootElement
                .GetProperty("data")
                .GetArrayLength();

            // Test 2: Individual game statistics
            using var statsResponse =
                await _httpClient.GetAsync(
                    "v1/stats?seasons[]=2026&per_page=10");

            if (!statsResponse.IsSuccessStatusCode)
            {
                return $"Active Players API SUCCESS ({playerCount} players). " +
                       $"Game Stats API failed: " +
                       $"{(int)statsResponse.StatusCode} " +
                       $"{statsResponse.ReasonPhrase}";
            }

            using var statsJson =
                JsonDocument.Parse(
                    await statsResponse.Content.ReadAsStringAsync());

            int statsCount = statsJson.RootElement
                .GetProperty("data")
                .GetArrayLength();

            return $"ALL-STAR API SUCCESS! " +
                   $"Active Players: {playerCount} records. " +
                   $"Game Stats: {statsCount} records.";
        }
        catch (Exception ex)
        {
            return $"API TEST ERROR: {ex.Message}";
        }
    }

    //BallDontLie API retrieve active player lineup code, each pull can only return 100 players, need to pull each page of 100 players for all actives.
    private async Task<List<JsonElement>> GetAllActivePlayersAsync()
    {
        var allPlayers = new List<JsonElement>();

        int? cursor = null;
        var usedCursors = new HashSet<int>();

        do
        {
            string url = "v1/players/active?per_page=100";

            if (cursor.HasValue)
            {
                url += $"&cursor={cursor.Value}";
            }

            using var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            await using var stream =
                await response.Content.ReadAsStreamAsync();

            using var document =
                await JsonDocument.ParseAsync(stream);

            var root = document.RootElement;

            var players = root.GetProperty("data");

            foreach (var player in players.EnumerateArray())
            {
                // Clone so the record remains usable after
                // the JSON document is disposed.
                allPlayers.Add(player.Clone());
            }

            // Look for the cursor to the next page.
            cursor = null;

            if (root.TryGetProperty("meta", out var meta) &&
                meta.TryGetProperty("next_cursor", out var next) &&
                next.ValueKind == JsonValueKind.Number)
            {
                cursor = next.GetInt32();

                if (!usedCursors.Add(cursor.Value))
                {
                    throw new InvalidOperationException(
                        "BALLDONTLIE returned a repeated cursor.");
                }
            }

            // Space out consecutive API requests.
            if (cursor.HasValue)
            {
                await Task.Delay(1200);
            }

        } while (cursor.HasValue);

        if (allPlayers.Count == 0)
        {
            throw new InvalidOperationException(
                "The active-player API returned no players.");
        }

        return allPlayers;
    }

    private async Task<List<JsonElement>> GetGameStatsForDateAsync(
        DateOnly date)
    {
        var allStats = new List<JsonElement>();
        var seenCursors = new HashSet<int>();
        int? cursor = null;

        do
        {
            string url =
                $"v1/stats?dates[]={date:yyyy-MM-dd}" +
                "&seasons[]=2026" +
                "&season_type=regular" +
                "&per_page=100";

            if (cursor.HasValue)
                url += $"&cursor={cursor.Value}";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var stream =
                await response.Content.ReadAsStreamAsync();

            using var document =
                await JsonDocument.ParseAsync(stream);

            var root = document.RootElement;

            foreach (var stat in root.GetProperty("data")
                                     .EnumerateArray())
            {
                allStats.Add(stat.Clone());
            }

            cursor = null;

            if (root.TryGetProperty("meta", out var meta) &&
                meta.TryGetProperty("next_cursor", out var next) &&
                next.ValueKind == JsonValueKind.Number)
            {
                cursor = next.GetInt32();

                if (!seenCursors.Add(cursor.Value))
                    throw new InvalidOperationException(
                        "Repeated pagination cursor.");
            }

            if (cursor.HasValue)
                await Task.Delay(1200);

        } while (cursor.HasValue);

        return allStats;
    }


    private static int? GetNullableInt(
        JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out int result))
            return result;

        return null;
    }

    private static int GetRequiredInt(
        JsonElement item, string name)
    {
        return GetNullableInt(item, name)
            ?? throw new InvalidOperationException(
                $"Missing required integer: {name}");
    }


    public async Task<int> ImportGameStatsForDateAsync(
        DateOnly date)
    {
        // Fetch every page before writing to PostgreSQL.
        var apiStats = await GetGameStatsForDateAsync(date);


        if (apiStats.Count == 0)
        {
            // Some dates have no NBA regular-season games.
            // Continue the date-range import without an error.
            return 0;
        }


        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var players = await db.NbaPlayers
            .ToDictionaryAsync(p => p.BallDontLiePlayerId);

        var games = await db.NbaGames
            .ToDictionaryAsync(g => g.BallDontLieGameId);

        var existingStats = (await db.NbaPlayerGameStats
            .Include(s => s.Player)
            .Include(s => s.Game)
            .Where(s => s.Game.GameDate == date)
            .ToListAsync())
            .ToDictionary(s => (
                s.Player.BallDontLiePlayerId,
                s.Game.BallDontLieGameId
            ));

        int processed = 0;

        foreach (var item in apiStats)
        {
            var playerJson = item.GetProperty("player");
            var gameJson = item.GetProperty("game");
            var teamJson = item.GetProperty("team");

            int playerId = GetRequiredInt(playerJson, "id");
            int gameId = GetRequiredInt(gameJson, "id");
            int teamId = GetRequiredInt(teamJson, "id");

            string dateText = GetString(gameJson, "date");

            if (dateText.Length < 10)
                throw new InvalidOperationException(
                    "Missing game date.");

            DateOnly gameDate =
                DateOnly.Parse(dateText[..10]);

            if (gameDate != date)
                throw new InvalidOperationException(
                    "Returned game date does not match request.");

            if (!players.TryGetValue(playerId, out var player))
            {
                player = new NbaPlayer
                {
                    BallDontLiePlayerId = playerId,
                    IsActive = false,
                    FirstName = GetString(playerJson, "first_name"),
                    LastName = GetString(playerJson, "last_name"),
                    Position = GetString(playerJson, "position")
                };

                db.NbaPlayers.Add(player);
                players[playerId] = player;
            }

            if (!games.TryGetValue(gameId, out var game))
            {
                game = new NbaGame
                {
                    BallDontLieGameId = gameId
                };

                db.NbaGames.Add(game);
                games[gameId] = game;
            }

            game.GameDate = gameDate;
            game.Season = GetRequiredInt(gameJson, "season");
            game.Status = GetString(gameJson, "status");

            string statusState =
                GetString(gameJson, "status_state");

            game.IsFinal =
                statusState.Equals(
                    "final", StringComparison.OrdinalIgnoreCase)
                || (string.IsNullOrEmpty(statusState) &&
                    game.Status.Equals(
                        "Final", StringComparison.OrdinalIgnoreCase));

            game.HomeTeamId =
                GetRequiredInt(gameJson, "home_team_id");

            game.VisitorTeamId =
                GetRequiredInt(gameJson, "visitor_team_id");

            game.HomeTeamScore =
                GetNullableInt(gameJson, "home_team_score") ?? 0;

            game.VisitorTeamScore =
                GetNullableInt(gameJson, "visitor_team_score") ?? 0;

            // Do not save partial box scores.
            if (!game.IsFinal)
                continue;

            var key = (playerId, gameId);

            if (!existingStats.TryGetValue(key, out var stat))
            {
                stat = new NbaPlayerGameStat();

                db.NbaPlayerGameStats.Add(stat);
                existingStats[key] = stat;
            }

            // EF Core handles the internal foreign keys.
            stat.Player = player;
            stat.Game = game;
            stat.BallDontLieTeamId = teamId;

            stat.Minutes = GetString(item, "min");
            stat.Points = GetNullableInt(item, "pts");
            stat.Rebounds = GetNullableInt(item, "reb");
            stat.Assists = GetNullableInt(item, "ast");
            stat.Steals = GetNullableInt(item, "stl");
            stat.Blocks = GetNullableInt(item, "blk");
            stat.Turnovers = GetNullableInt(item, "turnover");

            stat.OffensiveRebounds =
                GetNullableInt(item, "oreb");
            stat.DefensiveRebounds =
                GetNullableInt(item, "dreb");

            stat.FieldGoalsMade =
                GetNullableInt(item, "fgm");
            stat.FieldGoalsAttempted =
                GetNullableInt(item, "fga");

            stat.ThreePointersMade =
                GetNullableInt(item, "fg3m");
            stat.ThreePointersAttempted =
                GetNullableInt(item, "fg3a");

            stat.FreeThrowsMade =
                GetNullableInt(item, "ftm");
            stat.FreeThrowsAttempted =
                GetNullableInt(item, "fta");

            stat.PersonalFouls =
                GetNullableInt(item, "pf");
            stat.PlusMinus =
                GetNullableInt(item, "plus_minus");

            processed++;
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return processed;
    }


    public async Task<int> ImportGameStatsRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        Func<DateOnly, int, Task>? onDayComplete = null)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException(
                "End date cannot be before start date.");
        }

        int days = endDate.DayNumber - startDate.DayNumber + 1;

        // Keep initial batches small and easy to verify.
        if (days > 7)
        {
            throw new ArgumentException(
                "Import a maximum of seven days at a time.");
        }

        int totalRecords = 0;

        for (var date = startDate;
             date <= endDate;
             date = date.AddDays(1))
        {
            // Reuse the working single-date importer.
            int count = await ImportGameStatsForDateAsync(date);

            totalRecords += count;

            // Notify the Games page that a day completed.
            if (onDayComplete != null)
            {
                await onDayComplete(date, count);
            }
        }

        return totalRecords;
    }

}
