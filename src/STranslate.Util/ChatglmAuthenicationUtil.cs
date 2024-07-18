using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace STranslate.Util;

public class ChatglmAuthenicationUtil
{
    public static string GenerateToken(string apiKey, int expSeconds)
    {
        var parts = apiKey.Split('.');
        if (parts.Length != 2) throw new ArgumentException("Invalid API key format.");

        var id = parts[0];
        var secret = parts[1];
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        if (keyBytes.Length < 32)
            // Extend the key to meet the minimum length requirement
            Array.Resize(ref keyBytes, 32);

        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var payload = new JwtPayload
        {
            { "api_key", id },
            { "exp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expSeconds },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var header = new JwtHeader(credentials)
        {
            { "sign_type", "SIGN" }
        };

        var token = new JwtSecurityToken(header, payload);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}