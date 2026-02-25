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

                // SignalR needs the token pulled from the Authorization header manually.
                // The Flutter client sends it as a standard Bearer header on both
                // the negotiate request and subsequent transports.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        if (!path.StartsWithSegments("/hubs"))
                        {
                            return Task.CompletedTask;
                        }

                        var authHeader = context.Request.Headers.Authorization.ToString();
                        if (!string.IsNullOrEmpty(authHeader) &&
                            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader["Bearer ".Length..].Trim();
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}