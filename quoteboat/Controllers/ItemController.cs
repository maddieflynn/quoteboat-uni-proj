using Microsoft.AspNetCore.Mvc;
using quoteboat.Services;
using quoteboat.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace quoteboat.Controllers;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes
[ApiController]
[Route("items")]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly ItemService _itemService;

    public ItemController(ItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemReadDto>>> GetItems([FromQuery] string? filter, [FromQuery] string? sort, [FromQuery] string? status)
    {
        var items = await _itemService.GetAllItems(filter, sort, status);
        return Ok(items);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemReadDto>> GetItem(int id)
    {
        var item = await _itemService.GetItemById(id);
        if (item == null)
        {
            return NotFound("Item not found.");
        }
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ItemReadDto>> CreateItem(ItemCruDto dto)
    {
        var created = await _itemService.CreateItem(dto);
        // can't return CreatedAtAction as the DTO doesn't include ItemId
        // could probably add another ItemDto for this but this works ok too
        // notice response type attribute changes as well for this controller as Ok returns 200, not 201
        return Ok(created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemReadDto>> UpdateItem(int id, ItemCruDto dto)
    {
        var updated = await _itemService.UpdateItem(id, dto);
        if (updated == null)
        {
            return NotFound("Unable to update as item not found.");
        }
        return Ok(updated);
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateItem(int id)
    {
        var success = await _itemService.DeactivateItem(id);
        if (!success)
        {
            return NotFound("Unable to deactivate as item not found.");
        }
        return Ok();
    }

    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateItem(int id)
    {
        var success = await _itemService.ReactivateItem(id);
        if (!success)
        {
            return NotFound("Unable to reactivate as item not found.");
        }
        return Ok();
    }
}