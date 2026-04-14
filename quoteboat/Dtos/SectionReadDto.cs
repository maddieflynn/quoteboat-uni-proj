namespace quoteboat.Dtos;

public class SectionReadDto
{
    public int SectionId { get; set; }
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    // nested QuoteItem list
    // all belonging to the section identified by FK
    // avoids multiple database queries, results in one join
    public List<QuoteItemReadDto> QuoteItems { get; set; } = new();
}