using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data.Entities;

namespace PlayBuilder.Data;

public class PlayBuilderDbContext : DbContext
{
    public PlayBuilderDbContext(DbContextOptions<PlayBuilderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionGame> CollectionGames => Set<CollectionGame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>()
            .HasIndex(g => new
            {
                g.System,
                g.Title,
                g.Region
            });

        modelBuilder.Entity<Game>()
            .HasIndex(g => g.SourcePath)
            .IsUnique();

        modelBuilder.Entity<Collection>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<CollectionGame>()
            .HasKey(cg => new { cg.CollectionId, cg.GameId });

        modelBuilder.Entity<CollectionGame>()
            .HasOne(cg => cg.Collection)
            .WithMany(c => c.Games)
            .HasForeignKey(cg => cg.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollectionGame>()
            .HasOne(cg => cg.Game)
            .WithMany()
            .HasForeignKey(cg => cg.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}