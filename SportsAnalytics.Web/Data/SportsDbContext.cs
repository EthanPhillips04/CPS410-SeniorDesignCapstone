
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

    // New player and team tables
    public DbSet<NbaPlayer> NbaPlayers
        => Set<NbaPlayer>();

    public DbSet<NbaTeam> NbaTeams
        => Set<NbaTeam>();

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
    }
}

