using Microsoft.EntityFrameworkCore;
using quoteboat.Data;
using quoteboat.Interfaces;
using quoteboat.Models;

namespace quoteboat.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly QuoteBoatContext _context;

    public RefreshTokenRepository(QuoteBoatContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetRefreshToken(string username, string token)
    {
        // find the user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);
        if (user == null)
        {
            return null;
        }
        // get the associated token
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user.UserId && rt.Token == token);
    }

    public async Task RevokeRefreshToken(RefreshToken refreshToken)
    {
        // change revocation status
        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    public async Task SaveRefreshToken(string username, string token)
    {
        // find the user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);
        if (user == null)
        {
            return;
        }
        // generate the token
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = user.UserId,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(1), 
            IsRevoked = false
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }
}