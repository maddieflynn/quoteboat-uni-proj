using quoteboat.Models;

namespace quoteboat.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetClientById(int id);
    Task<Client?> GetClientByEmail(string email);
    Task<List<Client>> GetAllClients(string? filter, string? sort);
    Task<Client> CreateClient(Client client);
    Task<Client> UpdateClient(Client client);
}