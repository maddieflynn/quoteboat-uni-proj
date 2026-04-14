using quoteboat.Models;

namespace quoteboat.Interfaces;

public interface IQuoteItemRepository
{
    Task<QuoteItem?> GetQuoteItemById(int id);
    Task<List<QuoteItem>> GetQuoteItemBySectionId(int sectionId);
    Task<QuoteItem> CreateQuoteItem(QuoteItem quoteItem);
    Task<QuoteItem> UpdateQuoteItem(QuoteItem quoteItem);
    Task DeleteQuoteItem(int id);
}