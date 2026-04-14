using quoteboat.Models;

namespace quoteboat.Interfaces;

public interface ISectionRepository
{
    Task<Section?> GetSectionById(int id);
    Task<List<Section>> GetSectionByQuoteId(int quoteId);
    Task<Section> CreateSection(Section section);
    Task<Section> UpdateSection(Section section);
    Task DeleteSection(int id);
}