using quoteboat.Interfaces;
using quoteboat.Models;
using quoteboat.Dtos;

namespace quoteboat.Services;

// refer to ClientController and ClientService for comments on controller/service syntax & attributes
public class QuoteService
{
    // this service uses the repositories of models other than its own
    // this is because the DTO includes fields from other (related) objects
    // such as the name and address of the client 
    // the idea behind this was to give a print-ready view of the quote, which would traditionally include letter-like details
    // sender, recipient, address, etc. 
    private readonly IQuoteRepository _quoteRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISectionRepository _sectionRepository;

    public QuoteService(
        IQuoteRepository quoteRepository,
        IClientRepository clientRepository,
        IUserRepository userRepository,
        ISectionRepository sectionRepository)
    {
        _quoteRepository = quoteRepository;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _sectionRepository = sectionRepository;
    }

    public async Task<List<QuoteReadDto>> GetAllQuotes(string? filter, string? sort)
    {
        var quotes = await _quoteRepository.GetAllQuotes(filter, sort);
        var result = new List<QuoteReadDto>();
        foreach (var q in quotes)
        {
            var client = await _clientRepository.GetClientById(q.ClientId);
            var user = await _userRepository.GetUserById(q.UserId);
            if (client == null || user == null)
            {
                continue;
            }
            result.Add(new QuoteReadDto
            {
                QuoteId = q.QuoteId,
                QuoteNumber = q.QuoteNumber,
                State = q.State,
                CreatedAt = q.CreatedAt,
                ClientFirstName = client.FirstName,
                ClientLastName = client.LastName,
                ClientPhysicalAddress = client.PhysicalAddress,
                UserFirstName = user.FirstName,
                UserLastName = user.LastName
            });
        }
        return result;
    }

    public async Task<QuoteReadDto?> GetQuoteById(int id)
    {
        var q = await _quoteRepository.GetQuoteById(id);
        if (q == null)
        {
            return null;
        }
        var client = await _clientRepository.GetClientById(q.ClientId);
        var user = await _userRepository.GetUserById(q.UserId);
        if (client == null || user == null)
        {
            return null;
        }
        return new QuoteReadDto
        {
            QuoteId = q.QuoteId,
            QuoteNumber = q.QuoteNumber,
            State = q.State,
            CreatedAt = q.CreatedAt,
            ClientFirstName = client.FirstName,
            ClientLastName = client.LastName,
            ClientPhysicalAddress = client.PhysicalAddress,
            UserFirstName = user.FirstName,
            UserLastName = user.LastName
        };
    }

    public async Task<QuoteReadDto> CreateQuote(QuoteCreateCloneDto dto)
    {
        // get the latest quote number
        var latestQuoteNumber = await _quoteRepository.GetLatestQuoteNumber();
        // first quote number will be 1 if no existing quote is found
        int nextQuoteNumber = 1;
        if (!string.IsNullOrEmpty(latestQuoteNumber))
        {
            // if a quotenumber is found, extract the integer part of the string
            var intPart = latestQuoteNumber.Replace("345BSL", "");
            // C# version of stoi() function - convert the string to integer
            var currQuoteNumber = int.Parse(intPart);
            // increment by 1 for the next quote 
            nextQuoteNumber = currQuoteNumber + 1;
        }
        // convert back to a string once extracted
        string nextNumber = nextQuoteNumber.ToString();
        // fill left 0 to 6 chars (i.e. 000001 or 000234)
        // allows for 100,000 unique quote numbers
        nextNumber = nextNumber.PadLeft(5, '0');
        // recreate the string - quote number must begin with 345BSL as per business rules
        string quoteNumber = "345BSL" + numberPart;
        var quote = new Quote
        {
            UserId = 1, // placeholder
            ClientId = dto.ClientId,
            // use the recreated string as the new quote number
            QuoteNumber = quoteNumber,
            State = "DRAFT"
        };
        var created = await _quoteRepository.CreateQuote(quote);
        var client = await _clientRepository.GetClientById(created.ClientId);
        var user = await _userRepository.GetUserById(created.UserId);
        return new QuoteReadDto
        {
            QuoteId = created.QuoteId,
            QuoteNumber = created.QuoteNumber,
            State = created.State,
            CreatedAt = created.CreatedAt,
            ClientFirstName = client!.FirstName,
            ClientLastName = client.LastName,
            ClientPhysicalAddress = client.PhysicalAddress,
            UserFirstName = user!.FirstName,
            UserLastName = user.LastName
        };
    }

    public async Task<QuoteReadDto?> UpdateQuote(int id, string state)
    {
        var existing = await _quoteRepository.GetQuoteById(id);
        // cannot update a non-existent quote
        // cannot update a quote unless it is in the DRAFT state
        if (existing == null || existing.State != "DRAFT")
        {
            return null;
        }
        existing.State = state;
        var updated = await _quoteRepository.UpdateQuote(existing);
        var client = await _clientRepository.GetClientById(updated.ClientId);
        var user = await _userRepository.GetUserById(updated.UserId);
        return new QuoteReadDto
        {
            QuoteId = updated.QuoteId,
            QuoteNumber = updated.QuoteNumber,
            State = updated.State,
            CreatedAt = updated.CreatedAt,
            ClientFirstName = client!.FirstName,
            ClientLastName = client.LastName,
            ClientPhysicalAddress = client.PhysicalAddress,
            UserFirstName = user!.FirstName,
            UserLastName = user.LastName
        };
    }

    public async Task<bool> DeleteQuote(int id)
    {
        var existing = await _quoteRepository.GetQuoteById(id);
        // same as above
        if (existing == null || existing.State != "DRAFT")
        {
            return false;
        }
        await _quoteRepository.DeleteQuote(id);
        return true;
    }

    public async Task<bool> SendQuote(int id)
    {
        var quote = await _quoteRepository.GetQuoteById(id);
        // DRAFT -> SENT is the valid state transition
        if (quote == null || quote.State != "DRAFT")
        {
            return false;
        }
        var sections = await _sectionRepository.GetSectionByQuoteId(id);
        // business rule states that quotes cannot proceed past DRAFT state unless they have at least one section
        if (sections.Count == 0)
        {
            return false;
        }
        quote.State = "SENT";
        await _quoteRepository.UpdateQuote(quote);
        return true;
    }

    public async Task<bool> AcceptQuote(int id)
    {
        var quote = await _quoteRepository.GetQuoteById(id);
        // SENT -> ACCEPTED is the valid state transition
        if (quote == null || quote.State != "SENT")
        {
            return false;
        }
        quote.State = "ACCEPTED";
        await _quoteRepository.UpdateQuote(quote);
        return true;
    }

    public async Task<bool> RejectQuote(int id)
    {
        var quote = await _quoteRepository.GetQuoteById(id);
        // SENT -> REJECTED is the valid state transition
        if (quote == null || quote.State != "SENT")
        {
            return false;
        }
        quote.State = "REJECTED";
        await _quoteRepository.UpdateQuote(quote);
        return true;
    }

    public async Task<QuoteReadDto?> CloneQuote(int id, QuoteCreateCloneDto dto)
    {
        var original = await _quoteRepository.GetQuoteById(id);
        // cannot clone a non-existent quote
        if (original == null)
        {
            return null;
        }
        // get the latest quote number
        var latestQuoteNumber = await _quoteRepository.GetLatestQuoteNumber();
        // first quote number will be 1 if no existing quote is found
        int nextQuoteNumber = 1;
        if (!string.IsNullOrEmpty(latestQuoteNumber))
        {
            // if a quotenumber is found, extract the integer part of the string
            var intPart = latestQuoteNumber.Replace("345BSL", "");
            // C# version of stoi() function - convert the string to integer
            var currQuoteNumber = int.Parse(intPart);
            // increment by 1 for the next quote 
            nextQuoteNumber = currQuoteNumber + 1;
        }
        // convert back to a string once extracted
        string nextNumber = nextQuoteNumber.ToString();
        // fill left 0 to 6 chars (i.e. 000001 or 000234)
        // allows for 100,000 unique quote numbers
        nextNumber = nextNumber.PadLeft(5, '0');
        // recreate the string - quote number must begin with 345BSL as per business rules
        string quoteNumber = "345BSL" + numberPart;
        var newQuote = new Quote
        {
            UserId = 1, // placeholder
            ClientId = dto.ClientId,
            QuoteNumber = quoteNumber,
            State = "DRAFT"
        };
        // create the new quote
        var createdQuote = await _quoteRepository.CreateQuote(newQuote);
        var client = await _clientRepository.GetClientById(createdQuote.ClientId);
        var user = await _userRepository.GetUserById(createdQuote.UserId);
        return new QuoteReadDto
        {
            QuoteId = createdQuote.QuoteId,
            QuoteNumber = createdQuote.QuoteNumber,
            State = createdQuote.State,
            CreatedAt = createdQuote.CreatedAt,
            ClientFirstName = client!.FirstName,
            ClientLastName = client.LastName,
            ClientPhysicalAddress = client.PhysicalAddress,
            UserFirstName = user!.FirstName,
            UserLastName = user.LastName
        };
    }
}