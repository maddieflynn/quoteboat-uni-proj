using quoteboat.Models;

namespace quoteboat.Interfaces;

public interface IUserRepository 
{
    Task<User?> GetUserById(int id);
    Task<User?> GetUserByEmail(string email);
    Task<List<User>> GetAllUsers(string? filter, string? sort);
    Task<User> CreateUser(User user);
    Task<User> UpdateUser(User user);
}