using DailyPlant.Library.Config;
using Microsoft.EntityFrameworkCore;
using DailyPlant.Library.Models;

namespace DailyPlant.Library.Data
{
    public class PlantDbContext : DbContext
    {
        public DbSet<Plant> Plants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath =  DatabaseConfig.GetDatabasePath();
            
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Plant>(entity =>
            {
                entity.ToTable("Plant");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.SeasonSowing).HasColumnName("seasonSowing");
                entity.Property(e => e.SeasonBloom).HasColumnName("seasonBloom");
                entity.Property(e => e.Zone).HasColumnName("zone");
                entity.Property(e => e.Water).HasColumnName("water");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Image).HasColumnName("image");
                entity.Property(e => e.Category).HasColumnName("category");
            });
        }
    }
}
