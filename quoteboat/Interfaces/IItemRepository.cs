using quoteboat.Models;

namespace quoteboat.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetItemById(int id);
    Task<List<Item>> GetAllItems(string? filter, string? sort, string? status);
    Task<Item> CreateItem(Item item);
    Task<Item> UpdateItem(Item item);
}