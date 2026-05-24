using quoteboat.Models;
using quoteboat.Dtos;

namespace quoteboat.Interfaces;

public interface IUserRepository 
{
    Task<User?> GetUserById(int id);
    Task<User?> GetUserByEmail(string email);
    Task<List<User>> GetAllUsers(string? filter, string? sort, string? status);
    Task<User?> CreateUser(UserCreateDto dto);
    Task<User?> UpdateUser(int id, UserUpdateDto dto);
    Task<bool> DeactivateUser(int id);
    Task<bool> ReactivateUser(int id);
}



