using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;

namespace quoteboat.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly QuoteBoatContext _context;

    public ItemRepository(QuoteBoatContext context)
    {
        _context = context;
    }

    public async Task<Item?> GetItemById(int id)
    {
        // filter for active items
        return await _context.Items.FirstOrDefaultAsync(i => i.ItemId == id && i.IsActive);
    }

    public async Task<List<Item>> GetAllItems(string? filter, string? sort)
    {
        // filter for active items
        var query = _context.Items.Where(i => i.IsActive);
        // users can filter by item Name, SupplierName, or Type
        if (!string.IsNullOrEmpty(filter))
        {
            // filtering should not be case sensitive - frustrating for the user
            var lowerFilter = filter.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(lowerFilter) ||
                i.SupplierName.ToLower().Contains(lowerFilter) ||
                i.Type.ToLower().Contains(lowerFilter));
        }
        // users can sort by UniPrice high to low or low to high
        if (!string.IsNullOrEmpty(sort))
        {
            switch (sort.ToLower())
            {
                case "price-high-low":
                    query = query.OrderByDescending(i => i.UnitPrice);
                    break;

                case "price-low-high":
                    query = query.OrderBy(i => i.UnitPrice);
                    break;
            }
        }
        return await query.ToListAsync();
    }

    // controller / dto will handle creation fields
    public async Task<Item> CreateItem(Item item)
    {
        // item is always set to active at creation
        item.IsActive = true;
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateItem(Item item)
    {
        var existingItem = await _context.Items.FindAsync(item.ItemId);
        // can't update a non-existent item
        if (existingItem == null)
        {
            return null!;
        }
        // allowed field updates
        // everything except PK and FK
        existingItem.Type = item.Type;
        existingItem.Name = item.Name;
        existingItem.SupplierName = item.SupplierName;
        existingItem.UnitPrice = item.UnitPrice;
        existingItem.IsActive = item.IsActive;
        await _context.SaveChangesAsync();
        return existingItem;
    }
}