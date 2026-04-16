using quoteboat.Models;

namespace quoteboat.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshToken(string username, string token);
    Task RevokeRefreshToken(RefreshToken refreshToken);
    Task SaveRefreshToken(string username, string token);
}