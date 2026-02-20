using BackSharedGroceries.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BackSharedGroceries.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures the test application to use the PostgreSQL Testcontainer.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
        
        // Set required JWT environment variables for testing
        Environment.SetEnvironmentVariable("JWT_KEY", "test-secret-key-with-at-least-32-characters-for-hmac-sha256");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext configuration
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            // Add DbContext with the test PostgreSQL connection
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            // Ensure the database is created and migrations are applied
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        });
    }
}
