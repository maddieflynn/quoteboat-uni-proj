using System.IdentityModel.Tokens.Jwt;
using quoteboat.Dtos;
using quoteboat.Models;
using quoteboat.Other;
using quoteboat.Repositories;
using Microsoft.AspNetCore.Http;

namespace quoteboat.Services;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes

public class UserService
{
    private readonly UserRepository _userRepository;
    // source: https://stackoverflow.com/questions/50580232/get-userid-from-jwt-on-all-controller-methods [Larissa Savchekoo]
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(UserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    // source: https://stackoverflow.com/questions/50580232/get-userid-from-jwt-on-all-controller-methods [Larissa Savchekoo]
    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(i => i.Type == JwtRegisteredClaimNames.Sub)?.Value;
    }

    public async Task<List<UserReadDto>> GetAllUsers(string? filter, string? sort)
    {
        var users = await _userRepository.GetAllUsers(filter, sort);
        var result = new List<UserReadDto>();
        foreach (var u in users)
        {
            result.Add(new UserReadDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive
            });
        }
        return result;
    }

    public async Task<UserReadDto?> GetUserById(int id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null) return null;
        return new UserReadDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive
        };
    }

    public async Task<UserReadDto?> CreateUser(UserCreateDto dto)
    {
        var existingUser = await _userRepository.GetUserByEmail(dto.Email);
        if (existingUser != null)
        {
            return null;
        }
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            IsActive = true
        };
        var created = await _userRepository.CreateUser(user);
        return new UserReadDto
        {
            UserId = created.UserId,
            FirstName = created.FirstName,
            LastName = created.LastName,
            Email = created.Email,
            IsActive = created.IsActive
        };
    }

    public async Task<UserReadDto?> UpdateUser(int id, UserUpdateDto dto)
    {
        // get current user id (logged-in user)
        var userId = GetUserId();
        if (userId == null) return null;
        // parse to integer type
        var currentUserId = int.Parse(userId);
        // business logic: user can only modify themselves
        if (currentUserId != id) return null;
        var user = await _userRepository.GetUserById(id);
        if (user == null) return null;
        var emailUser = await _userRepository.GetUserByEmail(dto.Email);
        // email is already in use, cannot update 
        if (emailUser != null && emailUser.UserId != id)
        {
            return null;
        }
        // checks passed
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.IsActive = dto.IsActive;
        var updated = await _userRepository.UpdateUser(user);
        return new UserReadDto
        {
            UserId = updated.UserId,
            FirstName = updated.FirstName,
            LastName = updated.LastName,
            Email = updated.Email,
            IsActive = updated.IsActive
        };
    }

    public async Task<bool> DeactivateUser(int id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null)
        {
            return false;
        }
        user.IsActive = false;
        await _userRepository.UpdateUser(user);
        return true;
    }

    public async Task<bool> ReactivateUser(int id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null)
        {
            return false;
        }
        user.IsActive = true;
        await _userRepository.UpdateUser(user);
        return true;
    }
}