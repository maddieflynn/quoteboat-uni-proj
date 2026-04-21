using Microsoft.AspNetCore.Mvc;
using quoteboat.Services;
using quoteboat.Dtos;

namespace quoteboat.Controllers;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes
[ApiController]
[Route("quotes")]
[Authorize]
public class QuoteController : ControllerBase
{
    private readonly QuoteService _quoteService;

    public QuoteController(QuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<QuoteReadDto>>> GetQuotes([FromQuery] string? filter, [FromQuery] string? sort)
    {
        var quotes = await _quoteService.GetAllQuotes(filter, sort);
        return Ok(quotes);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteReadDto>> GetQuote(int id)
    {
        var quote = await _quoteService.GetQuoteById(id);
        if (quote == null)
        {
            return NotFound("Quote not found.");
        }
        return Ok(quote);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteReadDto>> CreateQuote(QuoteCreateCloneDto dto)
    {
        var created = await _quoteService.CreateQuote(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteReadDto>> UpdateQuote(int id, string state)
    {
        var updated = await _quoteService.UpdateQuote(id, state);
        if (updated == null)
        {
            return NotFound("Unable to update as quote not found.");
        }
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        var success = await _quoteService.DeleteQuote(id);
        if (!success)
        {
            return NotFound("Unable to delete as quote not found.");
        }
        return Ok();
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendQuote(int id)
    {
        var success = await _quoteService.SendQuote(id);
        if (!success)
        {
            return BadRequest("Unable to change state as quote not found.");
        }
        return Ok();
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptQuote(int id)
    {
        var success = await _quoteService.AcceptQuote(id);
        if (!success)
        {
            return BadRequest("Unable to change state as quote not found.");
        }
        return Ok();
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectQuote(int id)
    {
        var success = await _quoteService.RejectQuote(id);
        if (!success)
        {
            return BadRequest("Unable to change state as quote not found.");
        }
        return Ok();
    }

    [HttpPost("{id}/clone")]
    public async Task<ActionResult<QuoteReadDto>> CloneQuote(int id, QuoteCreateCloneDto dto)
    {
        var cloned = await _quoteService.CloneQuote(id, dto);
        if (cloned == null)
        {
            return NotFound("Unable to clone as quote not found.");
        }
        return Ok(cloned);
    }
}