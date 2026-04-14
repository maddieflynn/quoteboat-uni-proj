using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;

namespace quoteboat.Repositories;

public class SectionRepository : ISectionRepository
{
    private readonly QuoteBoatContext _context;

    public SectionRepository(QuoteBoatContext context)
    {
        _context = context;
    }

    public async Task<Section?> GetSectionById(int id)
    {
        return await _context.Sections.FirstOrDefaultAsync(s => s.SectionId == id);
    }

    public async Task<List<Section>> GetSectionByQuoteId(int quoteId)
    {
        // must return a List so cannot forget ToList function
        return await _context.Sections.Where(s => s.QuoteId == quoteId).ToListAsync();
    }

    // controller + dto will handle the creation fields
    // most will occur automatically depending on where user is in the UI (i.e. the Quote they are working on)
    public async Task<Section> CreateSection(Section section)
    {
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();
        return section;
    }

    // business rules for updating the section will be handled by services
    // data access layer should be unaware of business logic
    public async Task<Section> UpdateSection(Section section)
    {
        var existingSection = await _context.Sections.FindAsync(section.SectionId);
        // cannot update non-existent section
        if (existingSection == null)
        {
            return null!;
        }
        // allowed fields for updating - excludes PK and FKs
        existingSection.Type = section.Type;
        existingSection.Name = section.Name;
        await _context.SaveChangesAsync();
        return existingSection;
    }

    public async Task DeleteSection(int id)
    {
        var section = await _context.Sections.FindAsync(id);
        // cannot delete a non-existent section
        // can just continue in this case
        if (section == null)
        {
            return;
        }
        _context.Sections.Remove(section);
        await _context.SaveChangesAsync();
    }
}