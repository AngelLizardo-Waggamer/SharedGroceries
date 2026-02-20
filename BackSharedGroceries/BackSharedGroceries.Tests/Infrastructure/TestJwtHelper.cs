using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using JwtConstants = BackSharedGroceries.Helpers.JWT.JwtConstants;

namespace BackSharedGroceries.Tests.Infrastructure;

/// <summary>
/// Helper class for generating JWT tokens for testing purposes.
/// </summary>
public static class TestJwtHelper
{
    /// <summary>
    /// Generates a test JWT token with the specified claims.
    /// </summary>
    /// <param name="userId">User ID to include in the token</param>
    /// <param name="username">Username to include in the token</param>
    /// <param name="deviceId">Device ID to include in the token</param>
    /// <param name="familyId">Optional Family ID to include in the token</param>
    /// <param name="expiresInMinutes">Token expiration time (default: 60 minutes)</param>
    /// <returns>JWT token string</returns>
    public static string GenerateTestToken(
        Guid userId, 
        string username, 
        Guid deviceId, 
        Guid? familyId = null,
        int expiresInMinutes = 60)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim("DeviceId", deviceId.ToString())
        };

        if (familyId.HasValue)
        {
            claims.Add(new Claim("FamilyId", familyId.ToString()!));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConstants.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtConstants.Issuer,
            audience: JwtConstants.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates an expired JWT token (useful for testing token expiration scenarios).
    /// </summary>
    /// <param name="userId">User ID to include in the token</param>
    /// <param name="username">Username to include in the token</param>
    /// <param name="deviceId">Device ID to include in the token</param>
    /// <param name="familyId">Optional Family ID to include in the token</param>
    /// <returns>Expired JWT token string</returns>
    public static string GenerateExpiredToken(
        Guid userId, 
        string username, 
        Guid deviceId, 
        Guid? familyId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim("DeviceId", deviceId.ToString())
        };

        if (familyId.HasValue)
        {
            claims.Add(new Claim("FamilyId", familyId.ToString()!));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConstants.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtConstants.Issuer,
            audience: JwtConstants.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(-10), // Expired 10 minutes ago
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
