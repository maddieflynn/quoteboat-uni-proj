using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;

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
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsActive);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        // filter for active users
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<List<User>> GetAllUsers(string? filter, string? sort)
    {
        // query returns list of active users (deactivated users should not show in search YET but may implement this later)
        var query = _context.Users.Where(u => u.IsActive);
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

    public async Task<User> CreateUser(User user)
    {
        // new user is always set to active
        user.IsActive = true;
        // CreatedAt is always set at creation
        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUser(User user)
    {
        var existingUser = await _context.Users.FindAsync(user.UserId);
        // avoid attempted updates on non-existent users
        if (existingUser == null)
        {
            return null!;
        }
        // only these four fields should be updated
        // later, PasswordHash field could be added but I am not quite sure about the logic for changing passwords yet
        // CreatedAt & UserId should not change
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.IsActive = user.IsActive;
        await _context.SaveChangesAsync();
        return existingUser;
    }
}