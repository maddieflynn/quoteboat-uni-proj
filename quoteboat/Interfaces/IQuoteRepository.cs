using quoteboat.Models;

namespace quoteboat.Interfaces;

public interface IQuoteRepository
{
    Task<Quote?> GetQuoteById(int id);
    Task<Quote?> GetQuoteByQuoteNum(string quoteNumber);
    Task<List<Quote>> GetQuoteByClientId(int clientId);
    Task<List<Quote>> GetAllQuotes(string? filter, string? sort);
    Task<Quote> CreateQuote(Quote quote);
    Task<Quote> UpdateQuote(Quote quote);
    Task DeleteQuote(int id);
    Task<string?> GetLatestQuoteNumber();
}