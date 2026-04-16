// https://medium.com/@rckks/jwt-authentication-with-bcrypt-password-hashing-in-net-core-8-a412cec0725c

namespace quoteboat.Other;
public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}