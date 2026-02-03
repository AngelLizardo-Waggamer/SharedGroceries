using System.Text;
using BackSharedGroceries.Helpers.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BackSharedGroceries.Controllers.Auth
{
    /// <summary>
    /// Class that contains the extension method to configure the JWT Authentication for Dependency Injection.
    /// </summary>
    public static class JWTAuthConfiguration
    {

        /// <summary>
        /// Configures the JWT Authentication for token veracity validation on incoming requests for endpoints marked as Authorized.
        /// </summary>
        public static void ConfigureJWTAuth(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtConstants.Issuer,
                    ValidAudience = JwtConstants.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConstants.SecretKey))
                };
            });
        }
    }
}