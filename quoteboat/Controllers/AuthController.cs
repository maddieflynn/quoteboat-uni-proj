using Microsoft.AspNetCore.Mvc;
using quoteboat.Interfaces;
using quoteboat.Services;
using quoteboat.Other; 
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

// https://medium.com/@rckks/jwt-authentication-with-bcrypt-password-hashing-in-net-core-8-a412cec0725c
// https://medium.com/@MatinGhanbari/building-a-secure-api-with-asp-net-core-jwt-and-refresh-tokens-03dac37b4055
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthController(JwtService jwtService, IUserRepository userRepository, IConfiguration config, IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _config = config;
        _refreshTokenRepository = refreshTokenRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetUserByEmail(request.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }
        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // same error message, don't notify potential hackers if they got a valid username
            return Unauthorized("Invalid username or password.");
        }
        // generate access token and refresh token
        var accessToken = _jwtService.GenerateAccessToken(user.UserId, user.Email);
        var refreshToken = _jwtService.GenerateRefreshToken();
        // save the refresh to db token on login
        await _refreshTokenRepository.SaveRefreshToken(user.Email, refreshToken);

        return Ok(new { AccessToken = accessToken,
                        RefreshToken = refreshToken});
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = int.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub).Value);
        var username = principal.FindFirst(JwtRegisteredClaimNames.UniqueName).Value;

        var savedRefreshToken = await _refreshTokenRepository.GetRefreshToken(username, request.RefreshToken);
        if (savedRefreshToken == null || savedRefreshToken.IsRevoked || savedRefreshToken.ExpiryDate <= DateTime.UtcNow)
        {
            return Unauthorized("Invalid refresh token");
        }

        var newAccessToken = _jwtService.GenerateAccessToken(userId, username);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepository.RevokeRefreshToken(savedRefreshToken);
        await _refreshTokenRepository.SaveRefreshToken(username, newRefreshToken);

        return Ok(new
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _config.GetSection("JwtSettings");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // We want to get claims from expired token
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class TokenRequest
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}