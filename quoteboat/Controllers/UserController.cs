using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quoteboat.Dtos;
using quoteboat.Interfaces;

namespace quoteboat.Services;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes

[Route("api/users")]
[ApiController]
[Authorize]

public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserReadDto>>> GetUsers([FromQuery] string? filter, [FromQuery] string? sort, [FromQuery] string? status)
    {
        var users = await _userRepository.GetAllUsers(filter, sort, status);
        return Ok(users);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReadDto>> GetUser(int id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserReadDto>> CreateUser(UserCreateDto dto)
    {
        var created = await _userRepository.CreateUser(dto);
        if (created == null)
        {
            return BadRequest("Unable to create as user with this email already exists.");
        }
        return CreatedAtAction(nameof(GetUser), new { id = created.UserId }, created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReadDto>> UpdateUser(int id, UserUpdateDto dto)
    {
        var user = await _userRepository.UpdateUser(id, dto);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var success = await _userRepository.DeactivateUser(id);
        if (!success)
        {
            return NotFound("Unable to deactivate as user not found.");
        }
        return Ok();
    }

    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateUser(int id)
    {
        var success = await _userRepository.ReactivateUser(id);
        if (!success)
        {
            return NotFound("Unable to reactivate as user not found.");
        }
        return Ok();
    }
}