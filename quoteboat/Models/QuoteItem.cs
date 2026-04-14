namespace quoteboat.Models;
public class QuoteItem
{
    public int QuoteItemId { get; set; }
    public int SectionId { get; set; }
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPriceAtCreation { get; set; }

}