using Microsoft.AspNetCore.Mvc;
using quoteboat.Services;
using quoteboat.Dtos;

namespace quoteboat.Controllers;

// controller syntax, attributes, from Microsoft Learn docs: https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0
[ApiController]
[Route("clients")]
// derive from ControllerBase, not Controller (the latter is for handling web pages not API requests)
public class ClientController : ControllerBase
{
    // dependency injection - clientService
    // an example of loose coupling in the .NET repository pattern
    private readonly ClientService _clientService;

    public ClientController(ClientService clientService)
    {
        _clientService = clientService;
    }

    // HTTP request type
    [HttpGet]
    // HTTP response type
    [ProducesResponseType(StatusCodes.Status200OK)]
    // filter + sort params passed by [FromQuery] 
    // this is a binding source attribute 
    // ALSO, the return type is a List of ClientReadDto - NOT Client objects
    // this is what the frontend receives, therefore you limit it to what you want displayed/accessible from the UI
    // This is why DTOs were defined, so use them
    public async Task<ActionResult<List<ClientReadDto>>> GetClients([FromQuery] string? filter, [FromQuery] string? sort)
    {
        var clients = await _clientService.GetAllClients(filter, sort);
        // returns a 200 response code + the object (in this case a list of ClientReadDtos)
        return Ok(clients); // if list is empty, this will be handled by the frontend 
                            // so one response code is sufficient 
    }

    // {id} is the ClientId passed by the URL /clients/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientReadDto>> GetClient(int id)
    {
        var client = await _clientService.GetClientById(id);
        // no client found in database when repo made access call
        if (client == null)
        {
            // NotFound() is a special method of the ControllerBase class which returns a 404 status code
            // sends the HTTP status code and text - can determine what to do with this in the frontend
            return NotFound("Client not found.");
        } 
        return Ok(client);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientReadDto>> CreateClient(ClientCreateUpdateDto dto)
    {
        var created = await _clientService.CreateClient(dto);
        // if null returned from service, the first 5 lines of the CreateClient() method in servive indicate that a client with the specified email
        // already exists (was found in the database by the repo access call)
        // this breaks the unique email rule, cannot continue
        if (created == null)
        {
            // BadRequest() is a special method of the ControllerBase class which returns a 400 status code
            // sends the HTTP status code and text - can determine what to do with this in the frontend
            return BadRequest("Unable to create as client with this email already exists.");
        }
        // special method that creates a CreatedAtActonResult object - produces a 201 Created HTTP response
        // params are:
           // 1. actionName (the name of the action to use for generating the URL) 
           // 2. routeValues (the route data to use for generating the URL)
           // 3. value (the content value to format in the entity body) 
        return CreatedAtAction(nameof(GetClient), new { id = created.ClientId }, created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientReadDto>> UpdateClient(int id, ClientCreateUpdateDto dto)
    {
        var updated = await _clientService.UpdateClient(id, dto);
        if (updated == null)
        {
            return NotFound("Unable to update as client not found.");
        }
        return Ok(updated);
    }

    // similar to above comment on HttpGet("{id}) but URL is now clients/{id}/deactivate
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateClient(int id)
    {
        // deactivate and reactivate functions are boolean return types, true if successful
        var success = await _clientService.DeactivateClient(id);
        if (!success)
        {
            return NotFound("Unable to deactivate as client not found.");
        }
        return Ok();
    }

    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateClient(int id)
    {
        var success = await _clientService.ReactivateClient(id);
        if (!success)
        {
            return NotFound("Unable to reactivate as client not found.");
        }
        return Ok();
    }
}