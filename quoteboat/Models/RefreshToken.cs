namespace quoteboat.Models;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
}