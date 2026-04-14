namespace quoteboat.Dtos;

public class ItemCruDto
{
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}