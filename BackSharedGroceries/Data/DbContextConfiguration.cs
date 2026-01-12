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

        /// <summary>
        /// Applies any pending migrations to the database. If the database does not exists, it will be created. Also, if the database already has the latest migration, no action is taken.
        /// It also catches correctly common exceptions and has a general exception handler that stops the application preventing it from running in an inconsistent state.
        /// </summary>
        /// <param name="scope"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ApplyMigrationsToDb(this IServiceScope scope)
        {
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                throw new InvalidOperationException("Failed to establish a connection to the database. Ensure that the connection string is defined correctly and that Db is running.",
                ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                "Failed to apply database migrations. The database may be in an inconsistent state.", 
                ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while applying database migrations.", ex);
            }
        }
    }
}