
using Microsoft.EntityFrameworkCore;
using SportsAnalytics.Web.Models;

namespace SportsAnalytics.Web.Data;

public class SportsDbContext : DbContext
{
    public SportsDbContext(
        DbContextOptions<SportsDbContext> options)
        : base(options)
    {
    }

    // Existing statistics table
    public DbSet<BasketballPlayer> BasketballPlayers
        => Set<BasketballPlayer>();

    // New player & team tables & stats table for NBA data
    public DbSet<NbaPlayer> NbaPlayers
        => Set<NbaPlayer>();

    public DbSet<NbaTeam> NbaTeams
        => Set<NbaTeam>();

    public DbSet<NbaGame> NbaGames
        => Set<NbaGame>();

    public DbSet<NbaPlayerGameStat> NbaPlayerGameStats
        => Set<NbaPlayerGameStat>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BasketballPlayer>()
            .HasIndex(p => new
            {
                p.NbaPlayerId,
                p.Season
            })
            .IsUnique();

        modelBuilder.Entity<NbaPlayer>()
            .HasIndex(p => p.BallDontLiePlayerId)
            .IsUnique();

        modelBuilder.Entity<NbaTeam>()
            .HasIndex(t => t.BallDontLieTeamId)
            .IsUnique();

        modelBuilder.Entity<NbaPlayer>()
            .HasOne(p => p.Team)
            .WithMany(t => t.Players)
            .HasForeignKey(p => p.NbaTeamId);

        // Each BALLDONTLIE game ID must be unique.
        modelBuilder.Entity<NbaGame>()
            .HasIndex(g => g.BallDontLieGameId)
            .IsUnique();

        // A player can have only one stat row per game.
        modelBuilder.Entity<NbaPlayerGameStat>()
            .HasIndex(s => new
            {
                s.NbaPlayerId,
                s.NbaGameId
            })
            .IsUnique();

        // Connect each stat to its player.
        modelBuilder.Entity<NbaPlayerGameStat>()
            .HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.NbaPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Connect each stat to its game.
        modelBuilder.Entity<NbaPlayerGameStat>()
            .HasOne(s => s.Game)
            .WithMany()
            .HasForeignKey(s => s.NbaGameId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}

