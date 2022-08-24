using Microsoft.EntityFrameworkCore;

namespace EFCore6.TemporalTables.API;

public class TemporalDbContext : DbContext
{
    public TemporalDbContext(DbContextOptions<TemporalDbContext> opt)
        : base(opt)
    {

    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductDataConfiguration());
    }
}
