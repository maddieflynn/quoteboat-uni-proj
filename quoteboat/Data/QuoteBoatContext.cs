using quoteboat.Models;
using Microsoft.EntityFrameworkCore;
namespace quoteboat.Data;
public class QuoteBoatContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Quote> Quotes { get; set; } = null!;
    public DbSet<Section> Sections { get; set; } = null!;
    public DbSet<QuoteItem> QuoteItems { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    // note to self - need to move this out of dbcontext later so password is not exposed
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    optionsBuilder.UseSqlServer(
        "Server=tcp:qb-server-uni-proj.database.windows.net,1433;Initial Catalog=quoteboat-db;Persist Security Info=False;User ID=qbadmin;Password=27@nn15ST123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
        options => options.EnableRetryOnFailure()
        );
    }

}

