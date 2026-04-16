using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;

namespace quoteboat.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly QuoteBoatContext _context;

    public QuoteRepository(QuoteBoatContext context)
    {
        _context = context;
    }

    public async Task<Quote?> GetQuoteById(int id)
    {
        return await _context.Quotes.FirstOrDefaultAsync(q => q.QuoteId == id);
    }

    public async Task<Quote?> GetQuoteByQuoteNum(string quoteNumber)
    {
        return await _context.Quotes.FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber);
    }

    public async Task<List<Quote>> GetQuoteByClientId(int clientId)
    {
        // must return a List so cannot forget the ToList function
        return await _context.Quotes.Where(q => q.ClientId == clientId).ToListAsync();
    }

    public async Task<List<Quote>> GetAllQuotes(string? filter, string? sort)
    {
        IQueryable<Quote> query = _context.Quotes;
        // can filter by QuoteNumber, State, and Client name (accessible by the ClientId FK)
        if (!string.IsNullOrEmpty(filter))
        {
            var lowerFilter = filter.ToLower();

            query = query.Where(q =>
                q.QuoteNumber.ToLower().Contains(lowerFilter) ||
                q.State.ToLower().Contains(lowerFilter) ||
                _context.Clients.Any(c =>
                    c.ClientId == q.ClientId &&
                    (c.FirstName.ToLower().Contains(lowerFilter) ||
                     c.LastName.ToLower().Contains(lowerFilter))
                )
            );
        }
        // can sort by Newest-Oldest or Oldest-Newest
        // i couldn't think of any other reasonable sorting without storing the price of the quote
        // may add this field later, not sure yet
        if (!string.IsNullOrEmpty(sort))
        {
            switch (sort.ToLower())
            {
                case "newest":
                    query = query.OrderByDescending(q => q.CreatedAt);
                    break;

                case "oldest":
                    query = query.OrderBy(q => q.CreatedAt);
                    break;
            }
        }
        else
        {
            // default sort by Newest-Oldest
            query = query.OrderByDescending(q => q.CreatedAt);
        }
        return await query.ToListAsync();
    }

    public async Task<Quote> CreateQuote(Quote quote)
    {
        // creation time added at creation always, not handled by dto/controller
        quote.CreatedAt = DateTime.UtcNow;
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();
        return quote;
    }

    // there are some additional business rules around updating/deleting quotes 
    // these will be enforced by the services layer
    // data access layer should be unaware of business rules
    public async Task<Quote> UpdateQuote(Quote quote)
    {
        var existingQuote = await _context.Quotes.FindAsync(quote.QuoteId);
        // cannot update a non-existent quote
        if (existingQuote == null)
        {
            return null!;
        }
        // only one allowed field for updating - state
        existingQuote.State = quote.State;
        await _context.SaveChangesAsync();
        return existingQuote;
    }

    public async Task DeleteQuote(int id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        //cannot delete a non-existent quote.
        // can just return as causes no issues
        if (quote == null)
        {
            return;
        }
        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();
    }

    // probably not 100% safe if 2 users were acting concurrently, but unlikely to occur in a small business
    // so suitable for this project, but would need some other solution for an app with larger scale
    public async Task<string?> GetLatestQuoteNumber()
    {
        return await _context.Quotes.OrderByDescending(q => q.CreatedAt).Select(q => q.QuoteNumber).FirstOrDefaultAsync();
    }
}