using Microsoft.AspNetCore.Mvc;
using quoteboat.Services;
using quoteboat.Dtos;
using System.Runtime.Versioning;

namespace quoteboat.Controllers;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes
[ApiController]
// blank to allow for URLs like quotes/{id}/sections
[Route("")]
[Authorize]
public class SectionController : ControllerBase
{
    private readonly SectionService _sectionService;

    public SectionController(SectionService sectionService)
    {
        _sectionService = sectionService;
    }

    [HttpGet("sections/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SectionReadDto>> GetSection(int id)
    {
        var section = await _sectionService.GetSectionById(id);
        if (section == null)
        {
            return NotFound("Section not found.");
        }
        return Ok(section);
    }

    [HttpGet("quotes/{id}/sections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SectionReadDto>>> GetSectionsByQuote(int id)
    {
        var sections = await _sectionService.GetSectionsByQuoteId(id);
        return Ok(sections);
    }

    [HttpPost("quotes/{id}/sections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SectionCreateUpdateDto>> AddSection(int id, SectionCreateUpdateDto dto)
    {
        var created = await _sectionService.AddSectionToQuote(id, dto);
        if (created == null)
        {
            return BadRequest("Unable to create section. Quote invalid or not in DRAFT state.");
        }
        return Ok(created);
    }

    [HttpPut("sections/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SectionCreateUpdateDto>> UpdateSection(int id, SectionCreateUpdateDto dto)
    {
        var updated = await _sectionService.UpdateSection(id, dto);
        if (updated == null)
        {
            return BadRequest("Unable to update section. Section invalid or Quote invalid or not in DRAFT state.");
        }
        return Ok(updated);
    }

    [HttpDelete("sections/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var success = await _sectionService.DeleteSection(id);
        if (!success)
        {
            return BadRequest("Unable to update section. Section invalid or Quote invalid or not in DRAFT state.");
        }
        return Ok();
    }
}