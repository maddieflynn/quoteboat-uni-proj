using quoteboat.Models;
using Microsoft.EntityFrameworkCore;
namespace quoteboat.Data;


public class QuoteBoatContext : DbContext
{
    public QuoteBoatContext(DbContextOptions<QuoteBoatContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Quote> Quotes { get; set; } = null!;
    public DbSet<Section> Sections { get; set; } = null!;
    public DbSet<QuoteItem> QuoteItems { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
}

