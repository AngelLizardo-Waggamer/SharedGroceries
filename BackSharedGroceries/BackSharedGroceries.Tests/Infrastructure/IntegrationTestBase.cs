using System.Net.Http.Headers;
using BackSharedGroceries.Data;
using BackSharedGroceries.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BackSharedGroceries.Tests.Infrastructure;

/// <summary>
/// Base class for integration tests. Provides common setup and utility methods.
/// </summary>
[Collection("Database")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgresTestcontainerFixture _fixture;
    protected CustomWebApplicationFactory _factory = null!;
    protected HttpClient _client = null!;
    protected AppDbContext _dbContext = null!;

    protected IntegrationTestBase(PostgresTestcontainerFixture fixture)
    {
        _fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory(_fixture.ConnectionString);
        _client = _factory.CreateClient();

        // Get the DbContext for test data seeding
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Clean the database before each test
        await CleanDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    /// <summary>
    /// Cleans all data from the database to ensure test isolation.
    /// </summary>
    protected virtual async Task CleanDatabaseAsync()
    {
        _dbContext.Products.RemoveRange(_dbContext.Products);
        _dbContext.ShoppingLists.RemoveRange(_dbContext.ShoppingLists);
        _dbContext.RefreshTokens.RemoveRange(_dbContext.RefreshTokens);
        _dbContext.Users.RemoveRange(_dbContext.Users);
        _dbContext.Families.RemoveRange(_dbContext.Families);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Sets the authorization header for the HTTP client with a test JWT token.
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Creates a test user in the database.
    /// </summary>
    protected async Task<User> CreateTestUserAsync(
        string username, 
        string password = "Test@123", 
        Guid? familyId = null, 
        Guid? deviceId = null)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FamilyId = familyId,
            CurrentDeviceId = deviceId
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Creates a test family in the database.
    /// </summary>
    protected async Task<Family> CreateTestFamilyAsync(string name, string inviteCode)
    {
        var family = new Family
        {
            FamilyName = name,
            FamilyInviteCode = inviteCode
        };

        _dbContext.Families.Add(family);
        await _dbContext.SaveChangesAsync();
        return family;
    }

    /// <summary>
    /// Creates a test shopping list in the database.
    /// </summary>
    protected async Task<ShoppingList> CreateTestShoppingListAsync(Guid familyId, string name = "Test List")
    {
        var list = new ShoppingList
        {
            FamilyId = familyId,
            Name = name
        };

        _dbContext.ShoppingLists.Add(list);
        await _dbContext.SaveChangesAsync();
        return list;
    }

    /// <summary>
    /// Creates a test product in the database.
    /// </summary>
    protected async Task<Product> CreateTestProductAsync(
        Guid listId, 
        string name, 
        Guid lastModifiedByUserId,
        int quantity = 1,
        Enums.ProductStatus status = Enums.ProductStatus.Pending,
        DateTime? clientTimestamp = null)
    {
        var product = new Product
        {
            ListId = listId,
            Name = name,
            Quantity = quantity.ToString(),
            Status = status,
            LastModifiedByUserId = lastModifiedByUserId,
            ClientTimestamp = clientTimestamp ?? DateTime.UtcNow
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// Creates a test refresh token for a user.
    /// </summary>
    protected async Task<RefreshToken> CreateTestRefreshTokenAsync(
        Guid userId, 
        DateTime? expiresAt = null, 
        bool isRevoked = false)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddYears(1),
            IsRevoked = isRevoked
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();
        return refreshToken;
    }

    /// <summary>
    /// Clears the DbContext change tracker to force fresh queries from the database.
    /// Call this before verifying data that was updated via ExecuteUpdateAsync or ExecuteDeleteAsync.
    /// </summary>
    protected void ClearChangeTracker()
    {
        _dbContext.ChangeTracker.Clear();
    }
}
