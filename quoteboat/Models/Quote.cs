namespace quoteboat.Models;
public class Quote
{
    public int QuoteId { get; set; }
    public int UserId { get; set; }
    public int ClientId { get; set; }
    public string QuoteNumber { get; set; } = null!;
    public string State { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}