using BordspelAvond.Models;
using Microsoft.EntityFrameworkCore;

public class BordspelDbContext : DbContext
{
    public BordspelDbContext(DbContextOptions<BordspelDbContext> options) : base(options) { }

    public DbSet<Game> Games { get; set; }
    public DbSet<GameNight> GameNights { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameNight>()
            .HasMany(g => g.PlayedGames)
            .WithMany(g => g.GameNights)
            .UsingEntity<Dictionary<string, object>>(
                "GameGameNight",
                j => j
                    .HasOne<Game>()
                    .WithMany()
                    .HasForeignKey("GameId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j
                    .HasOne<GameNight>()
                    .WithMany()
                    .HasForeignKey("GameNightId")
                    .OnDelete(DeleteBehavior.Restrict)
            );

        // Remove this foreign key configuration:
        // modelBuilder.Entity<GameNight>()
        //    .HasOne(gn => gn.Organizer)
        //    .WithMany()
        //    .HasForeignKey(gn => gn.OrganizerId)
        //    .OnDelete(DeleteBehavior.Restrict);
    }

}
