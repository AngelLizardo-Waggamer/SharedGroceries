using System.Security.Claims;
using BackSharedGroceries.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BackSharedGroceries.Controllers.Auth
{

    /// <summary>
    /// Static class that handles the JWT generation.
    /// </summary>
    public static class JWTHandler
    {
        /// <summary>
        /// Generates a JWT token for the given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GenerateToken(User user)
        {

            // Create the main claims that need to be contained in the JWT, which are the user ID, username and 
            // the current device ID. 
            List<Claim> claims =
            [
              new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
              new Claim(ClaimTypes.Name, user.Username),
              new Claim("DeviceId", user.CurrentDeviceId.ToString()!)
            ];

            // If the user already has a family assigned, which will be the case of a normal login (not after registration), add it
            // to the claims.
            if (user.FamilyId.HasValue)
            {
                claims.Add(new Claim("FamilyId", user.FamilyId.ToString()!));
            }

            // Create the signing credentials using the jwt secret stored in the environment variables.
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(JwtConstants.SecretKey));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            // Assemble the token itself.
            // It is important to note that the expiration is set to 1 year, that is because this app is intended to be used by an old person
            // that may find difficult to re-login frequently.
            JwtSecurityToken token = new(
                issuer: JwtConstants.Issuer,
                audience: JwtConstants.Audience,
                claims: claims,
                expires: DateTime.Now.AddYears(1),
                signingCredentials: creds
            );

            // Write the token to a string and return it.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}