using BordspelAvond.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class BordspelDbContext : DbContext
{
    public BordspelDbContext(DbContextOptions<BordspelDbContext> options) : base(options) { }

    public DbSet<Game> Games { get; set; }
    public DbSet<GameNight> GameNights { get; set; }
    public DbSet<GameNightParticipant> GameNightParticipants { get; set; }
    public DbSet<GameNightGame> GameNightGames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuratie voor de tussenklasse GameNightGame om GameNight en Game te verbinden
        modelBuilder.Entity<GameNightGame>()
            .HasKey(gng => new { gng.GameNightId, gng.GameId });

        modelBuilder.Entity<GameNightGame>()
            .HasOne(gng => gng.GameNight)
            .WithMany(gn => gn.GameNightGames)
            .HasForeignKey(gng => gng.GameNightId);

        modelBuilder.Entity<GameNightGame>()
            .HasOne(gng => gng.Game)
            .WithMany()
            .HasForeignKey(gng => gng.GameId);

        // Configuratie voor GameNightParticipant (deelnemers van een GameNight)
        modelBuilder.Entity<GameNightParticipant>()
            .HasKey(gp => new { gp.GameNightId, gp.UserId });

        modelBuilder.Entity<GameNightParticipant>()
            .HasOne(gp => gp.GameNight)
            .WithMany(g => g.Participants)
            .HasForeignKey(gp => gp.GameNightId);

        // ValueConverter voor GameGenre om de enum als string op te slaan
        modelBuilder.Entity<Game>()
            .Property(g => g.Genre)
            .HasConversion(new EnumToStringConverter<Game.GameGenre>());

        modelBuilder.Entity<Game>()
          .Property(g => g.Type)
          .HasConversion(new EnumToStringConverter<Game.GameType>());
    }
}
