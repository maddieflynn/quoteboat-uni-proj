namespace quoteboat.Dtos;

public class ClientCreateUpdateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhysicalAddress { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool IsActive { get; set; }
}