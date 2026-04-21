using quoteboat.Interfaces;
using quoteboat.Models;
using quoteboat.Dtos;

namespace quoteboat.Services;


// refer to ClientController and ClientService for comments on controller/service syntax & attributes
public class SectionService
{
    // section service must use quote repository as well as its own
    // this is to access information about the state of a quote in certain functions
    // i.e. to update a section, a quote must be in the DRAFT state
    private readonly ISectionRepository _sectionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IQuoteRepository _quoteRepository;

    public SectionService(ISectionRepository sectionRepository, IQuoteRepository quoteRepository, IHttpContextAccessor httpContextAccessor)
    {
        _sectionRepository = sectionRepository;
        _quoteRepository = quoteRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    // for source please see UserService.cs file
    {
        return _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(i => i.Type == JwtRegisteredClaimNames.Sub)?.Value;
    }

    public async Task<SectionReadDto?> GetSectionById(int id)
    {
        var section = await _sectionRepository.GetSectionById(id);
        if (section == null)
        {
            return null;
        }

        return new SectionReadDto
        {
            SectionId = section.SectionId,
            Type = section.Type,
            Name = section.Name
        };
    }

    public async Task<List<SectionReadDto>> GetSectionsByQuoteId(int quoteId)
    {
        var sections = await _sectionRepository.GetSectionByQuoteId(quoteId);
        var result = new List<SectionReadDto>();

        foreach (var s in sections)
        {
            result.Add(new SectionReadDto
            {
                SectionId = s.SectionId,
                Type = s.Type,
                Name = s.Name
            });
        }

        return result;
    }

    public async Task<SectionCreateUpdateDto?> AddSectionToQuote(int quoteId, SectionCreateUpdateDto dto)
    {
        var quote = await _quoteRepository.GetQuoteById(quoteId);
        // cannot add section to a non-existent quote
        // cannot add section to a quote that is not in DRAFT state
        if (quote == null || quote.State != "DRAFT")
        {
            return null;
        }
        var userId = GetUserId();
        if (userId == null) 
        {
            return null;
        }
        var section = new Section
        {
            QuoteId = quoteId,
            UserId = int.Parse(userId),
            Type = dto.Type,
            Name = dto.Name
        };
        var created = await _sectionRepository.CreateSection(section);
        return new SectionCreateUpdateDto
        {
            Type = created.Type,
            Name = created.Name
        };
    }

    public async Task<SectionCreateUpdateDto?> UpdateSection(int id, SectionCreateUpdateDto dto)
    {
        var existing = await _sectionRepository.GetSectionById(id);
        if (existing == null)
        {
            return null;
        }
        // same as above
        var quote = await _quoteRepository.GetQuoteById(existing.QuoteId);
        if (quote == null || quote.State != "DRAFT")
        {
            return null;
        }
        existing.Type = dto.Type;
        existing.Name = dto.Name;
        var updated = await _sectionRepository.UpdateSection(existing);
        return new SectionCreateUpdateDto
        {
            Type = updated.Type,
            Name = updated.Name
        };
    }

    public async Task<bool> DeleteSection(int id)
    {
        var existing = await _sectionRepository.GetSectionById(id);
        if (existing == null)
        {
            return false;
        }
        // same as above
        var quote = await _quoteRepository.GetQuoteById(existing.QuoteId);
        if (quote == null || quote.State != "DRAFT")
        {
            return false;
        }
        await _sectionRepository.DeleteSection(id);
        return true;
    }
}