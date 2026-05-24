using Microsoft.AspNetCore.Mvc;
using quoteboat.Services;
using quoteboat.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace quoteboat.Controllers;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes
[ApiController]
// blank to allow for URLs like /sections/{id}/quoteitems
[Route("")]
[Authorize]
public class QuoteItemController : ControllerBase
{
    private readonly QuoteItemService _quoteItemService;

    public QuoteItemController(QuoteItemService quoteItemService)
    {
        _quoteItemService = quoteItemService;
    }

    [HttpGet("sections/{id}/quoteitems")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<QuoteItemReadDto>>> GetQuoteItems(int id)
    {
        var items = await _quoteItemService.GetQuoteItemsBySectionId(id);
        return Ok(items);
    }

    [HttpPost("sections/{id}/quoteitems")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuoteItemReadDto>> AddQuoteItem(int id, QuoteItemCreateDto dto)
    {
        var created = await _quoteItemService.AddItemToSection(id, dto);
        if (created == null)
        {
            return BadRequest("Unable to add item. Section invalid, Item invalid, or Quote invalid or not in DRAFT state.");
        }
        return Ok(created);
    }

    [HttpPut("quoteitems/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteItemReadDto>> UpdateQuoteItem(int id, QuoteItemUpdateDto dto)
    {
        var updated = await _quoteItemService.UpdateQuoteItem(id, dto);
        if (updated == null)
        {
            return NotFound("Unable to update item. Section invalid, Item invalid, or Quote invalid or not in DRAFT state.");
        }
        return Ok(updated);
    }

    [HttpDelete("quoteitems/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuoteItem(int id)
    {
        var success = await _quoteItemService.DeleteQuoteItem(id);
        if (!success)
        {
            return NotFound("Unable to delete item. Section invalid, Item invalid, or Quote invalid or not in DRAFT state.");
        }
        return Ok();
    }
}