namespace quoteboat.Dtos;

public class QuoteReadDto
{
    public int QuoteId { get; set; }
    public string QuoteNumber { get; set; } = null!;
    public string State { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string ClientFirstName { get; set; } = null!;
    public string ClientLastName { get; set; } = null!;
    public string ClientPhysicalAddress { get; set; } = null!;
    public string UserFirstName { get; set; } = null!;
    public string UserLastName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string UserPhoneNumber { get; set; } = null!;
}