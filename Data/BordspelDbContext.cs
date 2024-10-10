using BordspelAvond.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
            .WithMany(g => g.GameNights);

  
        modelBuilder.Entity<GameNight>()
            .HasMany(g => g.Players)
            .WithMany();

 
    }
}
