using quoteboat.Interfaces;
using quoteboat.Models;
using quoteboat.Dtos;
using System.IdentityModel.Tokens.Jwt;

namespace quoteboat.Services;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes
public class QuoteItemService
{
    // service needs access to multiple repositories
    // quote to check if quote state is DRAFT before making changes
    // section to check if a parent section exists
    // item to check if a parent item exists
    private readonly IQuoteItemRepository _quoteItemRepository;
    private readonly ISectionRepository _sectionRepository;
    private readonly IQuoteRepository _quoteRepository;
    private readonly IItemRepository _itemRepository;

    public QuoteItemService(
        IQuoteItemRepository quoteItemRepository,
        ISectionRepository sectionRepository,
        IQuoteRepository quoteRepository,
        IItemRepository itemRepository)
    {
        _quoteItemRepository = quoteItemRepository;
        _sectionRepository = sectionRepository;
        _quoteRepository = quoteRepository;
        _itemRepository = itemRepository;
    }

    public async Task<List<QuoteItemReadDto>> GetQuoteItemsBySectionId(int sectionId)
    {
        var items = await _quoteItemRepository.GetQuoteItemBySectionId(sectionId);
        var result = new List<QuoteItemReadDto>();
        foreach (var qi in items)
        {
            var item = await _itemRepository.GetItemById(qi.ItemId);
            // see top for comment
            if (item == null)
            {
                continue;
            }
            result.Add(new QuoteItemReadDto
            {
                QuoteItemId = qi.QuoteItemId,
                ItemId = qi.ItemId,
                SectionId = qi.SectionId,
                ItemName = item.Name,
                Type = item.Type,
                SupplierName = item.SupplierName,
                Quantity = qi.Quantity,
                UnitPriceAtCreation = qi.UnitPriceAtCreation
            });
        }
        return result;
    }

    public async Task<QuoteItemReadDto?> AddItemToSection(int sectionId, QuoteItemCreateDto dto)
    {
        // business rule - quantity must be greater than 0
        if (dto.Quantity <= 0)
        {
            return null;
        }
        // see top for comments
        var section = await _sectionRepository.GetSectionById(sectionId);
        if (section == null)
        {
            return null;
        }
        var quote = await _quoteRepository.GetQuoteById(section.QuoteId);
        if (quote == null || quote.State != "Draft")
        {
            return null;
        }
        var item = await _itemRepository.GetItemById(dto.ItemId);
        if (item == null)
        {
            return null;
        }
        var quoteItem = new QuoteItem
        {
            SectionId = sectionId,
            ItemId = dto.ItemId,
            Quantity = dto.Quantity,
            UnitPriceAtCreation = item.UnitPrice
        };
        var created = await _quoteItemRepository.CreateQuoteItem(quoteItem);
        return new QuoteItemReadDto
        {
            QuoteItemId = created.QuoteItemId,
            ItemId = created.ItemId,
            SectionId = created.SectionId,
            ItemName = item.Name,
            Type = item.Type,
            SupplierName = item.SupplierName,
            Quantity = created.Quantity,
            UnitPriceAtCreation = created.UnitPriceAtCreation
        };
    }

    public async Task<QuoteItemReadDto?> UpdateQuoteItem(int id, QuoteItemUpdateDto dto)
    {
        // business rule - quantity must be greater than 0
        if (dto.Quantity <= 0)
        {
            return null;
        }
        // see top for comment
        var existing = await _quoteItemRepository.GetQuoteItemById(id);
        if (existing == null)
        {
            return null;
        }
        var section = await _sectionRepository.GetSectionById(existing.SectionId);
        if (section == null)
        {
            return null;
        }
        var quote = await _quoteRepository.GetQuoteById(section.QuoteId);
        if (quote == null || quote.State != "Draft")
        {
            return null;
        }
        existing.Quantity = dto.Quantity;
        var updated = await _quoteItemRepository.UpdateQuoteItem(existing);
        var item = await _itemRepository.GetItemById(updated.ItemId);
        return new QuoteItemReadDto
        {
            QuoteItemId = updated.QuoteItemId,
            ItemId = updated.ItemId,
            SectionId = updated.SectionId,
            ItemName = item!.Name,
            Type = item.Type,
            SupplierName = item.SupplierName,
            Quantity = updated.Quantity,
            UnitPriceAtCreation = updated.UnitPriceAtCreation
        };
    }

    public async Task<bool> DeleteQuoteItem(int id)
    {
        var existing = await _quoteItemRepository.GetQuoteItemById(id);
        // see top for comment
        if (existing == null)
        {
            return false;
        }
        var section = await _sectionRepository.GetSectionById(existing.SectionId);
        if (section == null)
        {
            return false;
        }
        var quote = await _quoteRepository.GetQuoteById(section.QuoteId);
        if (quote == null || quote.State != "Draft")
        {
            return false;
        }
        await _quoteItemRepository.DeleteQuoteItem(id);
        return true;
    }
}