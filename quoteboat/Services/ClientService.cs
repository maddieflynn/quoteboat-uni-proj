using quoteboat.Interfaces;
using quoteboat.Models;
using quoteboat.Dtos;

namespace quoteboat.Services;

public class ClientService
{
    // dependency injection - clientRepository
    private readonly IClientRepository _clientRepository;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientService(IClientRepository clientRepository, IHttpContextAccessor httpContextAccessor)
    {
        _clientRepository = clientRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    // for source please see UserService.cs file
    {
        return _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(i => i.Type == JwtRegisteredClaimNames.Sub)?.Value;
    }

    // use DTOs instead of actual models as this is what the user is reading/accessing
    public async Task<List<ClientReadDto>> GetAllClients(string? filter, string? sort)
    {
        // LINQ query in the repo will search names, email with filter param
        // and will sort A-Z or Z-A or Newest-Oldest based on sort param
        var clients = await _clientRepository.GetAllClients(filter, sort);
        // clients is a list of full Client objects - repo is just straight data from the database, no DTOs
        // create a list of DTOs instead, build that off the list of Client objects returned from the repo
        var result = new List<ClientReadDto>();
        foreach (var c in clients)
        {
            result.Add(new ClientReadDto
            {
                // only add fields that the ClientReadDto object uses
                ClientId = c.ClientId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                PhysicalAddress = c.PhysicalAddress,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                IsActive = c.IsActive
            });
        }
        return result;
    }

    // ? used as could return null
    public async Task<ClientReadDto?> GetClientById(int id)
    {
        // call on the repo
        var client = await _clientRepository.GetClientById(id);
        if (client == null)
        {
            return null;
        }
        // again, need to recreate the result from the repo to match the DTO
        return new ClientReadDto
        {
            ClientId = client.ClientId,
            FirstName = client.FirstName,
            LastName = client.LastName,
            PhysicalAddress = client.PhysicalAddress,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            IsActive = client.IsActive
        };
    }

    public async Task<ClientReadDto?> CreateClient(ClientCreateUpdateDto dto)
    {
        var existing = await _clientRepository.GetClientByEmail(dto.Email);
        if (existing != null)
        {
            return null;
        }
        // create a new Client object
        // use DTO fields, repo will handle the rest

        // get logged in user
        var userId = GetUserId();
        if (userId == null) 
        {
            return null;
        }
        var client = new Client
        {
            UserId = int.Parse(userId),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhysicalAddress = dto.PhysicalAddress,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            IsActive = dto.IsActive
        };
        // call repo to add the new Client object to the database
        var created = await _clientRepository.CreateClient(client);
        // return the details of the new Client via a ReadDto
        return new ClientReadDto
        {
            ClientId = created.ClientId,
            FirstName = created.FirstName,
            LastName = created.LastName,
            PhysicalAddress = created.PhysicalAddress,
            Email = created.Email,
            PhoneNumber = created.PhoneNumber,
            IsActive = created.IsActive
        };
    }

    public async Task<ClientReadDto?> UpdateClient(int id, ClientCreateUpdateDto dto)
    {
        var existing = await _clientRepository.GetClientById(id);
        if (existing == null)
        {
            return null;
        }
        existing.FirstName = dto.FirstName;
        existing.LastName = dto.LastName;
        existing.PhysicalAddress = dto.PhysicalAddress;
        existing.Email = dto.Email;
        existing.PhoneNumber = dto.PhoneNumber;
        existing.IsActive = dto.IsActive;
        var updated = await _clientRepository.UpdateClient(existing);
        return new ClientReadDto
        {
            ClientId = updated.ClientId,
            FirstName = updated.FirstName,
            LastName = updated.LastName,
            PhysicalAddress = updated.PhysicalAddress,
            Email = updated.Email,
            PhoneNumber = updated.PhoneNumber,
            IsActive = updated.IsActive
        };
    }

    public async Task<bool> DeactivateClient(int id)
    {
        var client = await _clientRepository.GetClientById(id);
        if (client == null)
        {
            return false;
        }
        // only change one field
        client.IsActive = false;
        await _clientRepository.UpdateClient(client);
        return true;
    }

    public async Task<bool> ReactivateClient(int id)
    {
        var client = await _clientRepository.GetClientById(id);
        if (client == null)
        {
            return false;
        }
        client.IsActive = true;
        await _clientRepository.UpdateClient(client);
        return true;
    }
}