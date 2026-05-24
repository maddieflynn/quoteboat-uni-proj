using quoteboat.Interfaces;
using quoteboat.Models;
using quoteboat.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace quoteboat.Services;


// refer to ClientController and ClientService for comments on controller/service syntax & attributes
public class ItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ItemService(IItemRepository itemRepository, IHttpContextAccessor httpContextAccessor)
    {
        _itemRepository = itemRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<List<ItemReadDto>> GetAllItems(string? filter, string? sort, string? status)
    {
        var items = await _itemRepository.GetAllItems(filter, sort, status);
        var result = new List<ItemReadDto>();
        foreach (var i in items)
        {
            result.Add(new ItemReadDto
            {
                ItemId = i.ItemId,
                Type = i.Type,
                Name = i.Name,
                SupplierName = i.SupplierName,
                UnitPrice = i.UnitPrice,
                IsActive = i.IsActive
            });
        }
        return result;
    }

    public async Task<ItemReadDto?> GetItemById(int id)
    {
        var item = await _itemRepository.GetItemById(id);
        if (item == null)
        {
            return null;
        }
        return new ItemReadDto
        {
            ItemId = item.ItemId,
            Type = item.Type,
            Name = item.Name,
            SupplierName = item.SupplierName,
            UnitPrice = item.UnitPrice,
            IsActive = item.IsActive
        };
    }

    public async Task<ItemReadDto?> CreateItem(ItemCruDto dto)
    {
        var userId = GetUserId();
        Console.WriteLine(userId);
        if (userId == null) 
        {
            return null;
        }
        var item = new Item
        {
            UserId = int.Parse(userId),
            Type = dto.Type,
            Name = dto.Name,
            SupplierName = dto.SupplierName,
            UnitPrice = dto.UnitPrice,
            IsActive = dto.IsActive
        };
        var created = await _itemRepository.CreateItem(item);
        return new ItemReadDto
        {
            ItemId = created.ItemId,
            Type = created.Type,
            Name = created.Name,
            SupplierName = created.SupplierName,
            UnitPrice = created.UnitPrice,
            IsActive = created.IsActive
        };
    }

    public async Task<ItemReadDto?> UpdateItem(int id, ItemCruDto dto)
    {
        var existing = await _itemRepository.GetItemById(id);
        if (existing == null)
        {
            return null;
        }
        existing.Type = dto.Type;
        existing.Name = dto.Name;
        existing.SupplierName = dto.SupplierName;
        existing.UnitPrice = dto.UnitPrice;
        var updated = await _itemRepository.UpdateItem(existing);
        return new ItemReadDto
        {
            ItemId = updated.ItemId,
            Type = updated.Type,
            Name = updated.Name,
            SupplierName = updated.SupplierName,
            UnitPrice = updated.UnitPrice,
            IsActive = updated.IsActive
        };
    }

    public async Task<bool> DeactivateItem(int id)
    {
        var item = await _itemRepository.GetItemById(id);
        if (item == null)
        {
            return false;
        }
        item.IsActive = false;
        await _itemRepository.UpdateItem(item);
        return true;
    }

    public async Task<bool> ReactivateItem(int id)
    {
        var item = await _itemRepository.GetItemById(id);
        if (item == null)
        {
            return false;
        }
        item.IsActive = true;
        await _itemRepository.UpdateItem(item);
        return true;
    }
}