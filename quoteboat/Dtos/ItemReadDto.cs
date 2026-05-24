namespace quoteboat.Dtos;
public class ItemReadDto
{
    public int ItemId { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string SupplierName { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}