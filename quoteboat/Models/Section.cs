namespace quoteboat.Models;
public class Section


{
    public int SectionId { get; set; }
    public int QuoteId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;

}