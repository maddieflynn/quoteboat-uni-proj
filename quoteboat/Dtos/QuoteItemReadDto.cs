namespace quoteboat.Dtos;

public class QuoteItemReadDto
{
    public int ItemId { get; set; }
    // extracted by a join to the Item table - handled in services
    // does not belong to the QuoteItem model
    public string ItemName { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPriceAtCreation { get; set; }
}