using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Interfaces.Services;
using BackSharedGroceries.Repositories;
using BackSharedGroceries.Services;

namespace BackSharedGroceries.Interfaces
{
    /// <summary>
    /// Class that contains the extension method to configure the Interfaces for Dependency Injection.
    /// </summary>
    public static class InterfacesConfiguration
    {

        /// <summary>
        /// Configures the Interfaces of the Repositories and Services for Dependency Injection.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureInterfaces(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Register services
            services.AddScoped<IAuthService, AuthService>();
        }
    }
}