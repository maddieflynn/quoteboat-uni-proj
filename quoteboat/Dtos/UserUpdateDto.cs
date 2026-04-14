namespace quoteboat.Dtos;

public class UserUpdateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
}