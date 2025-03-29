using BloodBowl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodBowl.Domain.Context;

public class BloodBowlDbContext(DbContextOptions<BloodBowlDbContext> options) : DbContext(options)
{
    public DbSet<PlayerScore> PlayerScore { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }
}
