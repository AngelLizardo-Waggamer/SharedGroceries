using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace BackSharedGroceries.Tests.Infrastructure;

/// <summary>
/// Fixture that manages a PostgreSQL Testcontainer that is shared across all integration tests.
/// This implements IAsyncLifetime to properly initialize and dispose the container.
/// </summary>
public class PostgresTestcontainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;

    /// <summary>
    /// Gets the connection string for the PostgreSQL container.
    /// </summary>
    public string ConnectionString => _postgresContainer?.GetConnectionString() 
        ?? throw new InvalidOperationException("Container not initialized");

    /// <summary>
    /// Initializes the PostgreSQL container before any tests run.
    /// </summary>
    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("sharedgroceries_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithPortBinding(5432, true) // Bind to random host port
            .WithCleanUp(true) // Clean up container after tests
            .Build();

        await _postgresContainer.StartAsync();
    }

    /// <summary>
    /// Disposes the PostgreSQL container after all tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }
}

/// <summary>
/// Collection definition to enable sharing the PostgreSQL container across test classes.
/// Any test class decorated with [Collection("Database")] will share the same container instance.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresTestcontainerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionFixture<>] and all the
    // ICollectionFixture<> interfaces.
}
