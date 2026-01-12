using BackSharedGroceries.Data;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    /// <summary>
    /// Class that contains the extension method to configure the DbContext.
    /// </summary>
    public static class DbContextConfiguration
    {
        /// <summary>
        /// Configures the AppDbContext with the PostgreSQL db provider using the environment variable of the connection string, but uses
        /// the appsettings connection string as a fallback if the env variable is not set.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CN_STR")
                    ?? configuration.GetConnectionString("DefaultConnection"));
            });
        }
    }
}