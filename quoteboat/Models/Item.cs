
namespace quoteboat.Models;
public class Item 
{
    public int ItemId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;

}