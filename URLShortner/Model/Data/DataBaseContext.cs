using Microsoft.EntityFrameworkCore;
using URLShortner.Model;

namespace URLShortner.Model.Data;

public class DataBaseContext : DbContext
{
    public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
    {
    }

    public DbSet<URLModel> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<URLModel>().ToContainer("Items").HasPartitionKey(x => x.HashURL);
    }
}
