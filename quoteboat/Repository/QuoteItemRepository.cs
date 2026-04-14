using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;

namespace quoteboat.Repositories;

public class QuoteItemRepository : IQuoteItemRepository
{
    private readonly QuoteBoatContext _context;

    public QuoteItemRepository(QuoteBoatContext context)
    {
        _context = context;
    }

    public async Task<QuoteItem?> GetQuoteItemById(int id)
    {
        return await _context.QuoteItems.FirstOrDefaultAsync(qi => qi.QuoteItemId == id);
    }

    public async Task<List<QuoteItem>> GetQuoteItemBySectionId(int sectionId)
    {
        return await _context.QuoteItems
            .Where(qi => qi.SectionId == sectionId)
            .ToListAsync();
    }
    
    // controller/dto will handle the fields for creation
    public async Task<QuoteItem> CreateQuoteItem(QuoteItem quoteItem)
    {
        _context.QuoteItems.Add(quoteItem);
        await _context.SaveChangesAsync();
        return quoteItem;
    }

    public async Task<QuoteItem> UpdateQuoteItem(QuoteItem quoteItem)
    {
        var existingQuoteItem = await _context.QuoteItems.FindAsync(quoteItem.QuoteItemId);
        // don't update a non-existent QuoteItem
        if (existingQuoteItem == null)
        {
            return null!;
        }
        // only reasonable field to update is quantity.
        // other fields are either PK, FK, or the price snapshot, which should not change
        existingQuoteItem.Quantity = quoteItem.Quantity;
        await _context.SaveChangesAsync();
        return existingQuoteItem;
    }

    public async Task DeleteQuoteItem(int id)
    {
        var quoteItem = await _context.QuoteItems.FindAsync(id);
        // can't delete a non-existent quoteitem
        // doesn't need to return null as user can just continue
        if (quoteItem == null)
        {
            return;
        }
        _context.QuoteItems.Remove(quoteItem);
        await _context.SaveChangesAsync();
    }
}