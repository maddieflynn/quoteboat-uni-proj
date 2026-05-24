using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;

namespace quoteboat.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly QuoteBoatContext _context;

    public ClientRepository(QuoteBoatContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetClientById(int id)
    {
        // filter fpr active clients
        return await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == id);
    }

    public async Task<Client?> GetClientByEmail(string email)
    {
        // filter for active clients
        return await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<List<Client>> GetAllClients(string? filter, string? sort, string? status)
    {
        var query = _context.Clients.AsQueryable();
        if (status == "active")
        {
            query = query.Where(c => c.IsActive);
        }
        else if (status == "inactive")
        {
            query = query.Where(c => !c.IsActive);
        }
        // see comments in UserRepository for filtering and sorting
        if (!string.IsNullOrEmpty(filter))
        {
            var lowerFilter = filter.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(lowerFilter) ||
                c.LastName.ToLower().Contains(lowerFilter) ||
                c.Email.ToLower().Contains(lowerFilter));
        }

        if (!string.IsNullOrEmpty(sort))
        {
            switch (sort.ToLower())
            {
                case "a-z":
                    query = query
                        .OrderBy(c => c.FirstName)
                        .ThenBy(c => c.LastName);
                    break;

                case "z-a":
                    query = query
                        .OrderByDescending(c => c.FirstName)
                        .ThenByDescending(c => c.LastName);
                    break;

                case "newest":
                    query = query
                        .OrderByDescending(c => c.CreatedAt);
                    break;

                case "oldest":
                    query = query
                        .OrderBy(c => c.CreatedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }
        return await query.ToListAsync();
    }

    public async Task<Client> CreateClient(Client client)
    {
        // IsActive always set to true at creation
        client.IsActive = true;
        // time of creation recorded at creation
        client.CreatedAt = DateTime.UtcNow;
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
        // note: don't need to specify clientid, EF Core knows it is a primary key, dtaabase will autogenerate 
    }

    public async Task<Client> UpdateClient(Client client)
    {
        var existingClient = await _context.Clients.FindAsync(client.ClientId);
        // see UserRepository for comments on updating - same logic
        if (existingClient == null)
        {
            return null!;
        }
        // allowed field changes
        // excludes ClientId, UserId, CreatedAt
        existingClient.FirstName = client.FirstName;
        existingClient.LastName = client.LastName;
        existingClient.PhysicalAddress = client.PhysicalAddress;
        existingClient.Email = client.Email;
        existingClient.PhoneNumber = client.PhoneNumber;
        existingClient.IsActive = client.IsActive;
        await _context.SaveChangesAsync();
        return existingClient;
    }
}