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

        // Configureren van de many-to-many relatie tussen Game en GameNight zonder cascade delete
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

      
       
    }
}
