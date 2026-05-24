using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;
using quoteboat.Dtos;

namespace quoteboat.Repositories;

public class UserRepository : IUserRepository
{
    private readonly QuoteBoatContext _context;

    public UserRepository(QuoteBoatContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserById(int id)
    {
        //filter for active users
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        // filter for active users
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllUsers(string? filter, string? sort, string? status)
    {
        var query = _context.Users.AsQueryable();
        if (status == "active")
        {
            query = query.Where(u => u.IsActive);
        }
        else if (status == "inactive")
        {
            query = query.Where(u => !u.IsActive);
        }
        // filter by name or email
        if (!string.IsNullOrEmpty(filter))
        {
            // filtering should not be case sensitive - this would be frustrating for the user
            var lowerFilter = filter.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(lowerFilter) ||
                u.LastName.ToLower().Contains(lowerFilter) ||
                u.Email.ToLower().Contains(lowerFilter));
        }
        // sort by A-Z, Z-A, Newest-Oldest, or Oldest-Newest
        if (!string.IsNullOrEmpty(sort))
        {
            switch (sort.ToLower())
            {
                case "a-z":
                    query = query
                        .OrderBy(u => u.FirstName)
                        .ThenBy(u => u.LastName);
                    break;

                case "z-a":
                    query = query
                        .OrderByDescending(u => u.FirstName)
                        .ThenByDescending(u => u.LastName);
                    break;

                case "newest":
                    query = query
                        .OrderByDescending(u => u.CreatedAt);
                    break;

                case "oldest":
                    query = query
                        .OrderBy(u => u.CreatedAt);
                    break;
            }
        }
        else
        {
            // default sort by Newest-Oldest
            query = query.OrderByDescending(u => u.CreatedAt);
        }
        // ToList is required because the return type is a List
        return await query.ToListAsync();
    }

    public async Task<User?> CreateUser(UserCreateDto dto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        // no email double ups
        if (existingUser != null)
        {
            return null;
        }
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            // plain text password is hashed before it's stored
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            // CreatedAt is always set at creation
            CreatedAt = DateTime.UtcNow,
            // new user is always set to active
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUser(int id, UserUpdateDto dto)
    {
        var existingUser = await _context.Users.FindAsync(id);
        // avoid attempted updates on non-existent users
        if (existingUser == null)
        {
            return null;
        }
        existingUser.FirstName = dto.FirstName;
        existingUser.LastName = dto.LastName;
        existingUser.Email = dto.Email;
        existingUser.PhoneNumber = dto.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }
        await _context.SaveChangesAsync();
        return existingUser;
    }

    public async Task<bool> DeactivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        // avoid attempted deactivation on non-existent users
        if (user == null)
        {
            return false;
        }
        user.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
    // frontend UI doesn't actually have functionality for this yet but good to have in the API in case I need to reactivate a user
    // avoids having to interact directly with the DB in Azure
    public async Task<bool> ReactivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        // avoid attempted reactivation on non-existent users
        if (user == null)
        {
            return false;
        }
        user.IsActive = true;
        await _context.SaveChangesAsync();
        return true;
    }

}