using System.Net;
using System.Net.Http.Json;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Enums;
using BackSharedGroceries.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Tests.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the Products Controller.
/// Tests CRUD operations, sync engine, Last-Write-Wins conflict resolution,
/// cross-family isolation, and batch sync scenarios.
/// </summary>
public class ProductsControllerIntegrationTests : IntegrationTestBase
{
    public ProductsControllerIntegrationTests(PostgresTestcontainerFixture fixture) : base(fixture)
    {
    }

    #region Suggestions Privacy Tests

    [Fact]
    public async Task GetSuggestions_OnlyReturnsProductsFromUserFamily()
    {
        // Arrange
        // Create Family A with user and products
        var familyA = await CreateTestFamilyAsync("Family A", "FAMA001");
        var listA = await CreateTestShoppingListAsync(familyA.FamilyId);
        var deviceIdA = Guid.NewGuid();
        var userA = await CreateTestUserAsync("userA", "Password123", familyA.FamilyId, deviceIdA);
        await CreateTestProductAsync(listA.Id, "Milk", userA.Id);
        await CreateTestProductAsync(listA.Id, "Bread", userA.Id);

        // Create Family B with user and products
        var familyB = await CreateTestFamilyAsync("Family B", "FAMB001");
        var listB = await CreateTestShoppingListAsync(familyB.FamilyId);
        var deviceIdB = Guid.NewGuid();
        var userB = await CreateTestUserAsync("userB", "Password123", familyB.FamilyId, deviceIdB);
        await CreateTestProductAsync(listB.Id, "Eggs", userB.Id);
        await CreateTestProductAsync(listB.Id, "Cheese", userB.Id);

        // Authenticate as userA
        var tokenA = TestJwtHelper.GenerateTestToken(userA.Id, userA.Username, deviceIdA, familyA.FamilyId);
        SetAuthorizationHeader(tokenA);

        // Act
        var response = await _client.GetAsync("/api/products/v1/suggestions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var suggestions = await response.Content.ReadFromJsonAsync<List<string>>();
        suggestions.Should().NotBeNull();
        suggestions.Should().Contain("Milk");
        suggestions.Should().Contain("Bread");
        suggestions.Should().NotContain("Eggs"); // From Family B
        suggestions.Should().NotContain("Cheese"); // From Family B
    }

    [Fact]
    public async Task GetSuggestions_WhenUserHasNoFamily_ReturnsBadRequestOrEmpty()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("noFamilyUser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/products/v1/suggestions");

        // Assert
        // Based on your implementation, this should return BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Cross-Family Update Protection Tests

    [Fact]
    public async Task UpdateProduct_FromDifferentFamily_ReturnsForbiddenOrNotFound()
    {
        // Arrange
        // Create Family A with a product
        var familyA = await CreateTestFamilyAsync("Family A", "FAMA001");
        var listA = await CreateTestShoppingListAsync(familyA.FamilyId);
        var deviceIdA = Guid.NewGuid();
        var userA = await CreateTestUserAsync("userA", "Password123", familyA.FamilyId, deviceIdA);
        var productA = await CreateTestProductAsync(listA.Id, "Milk", userA.Id);

        // Create Family B with a different user
        var familyB = await CreateTestFamilyAsync("Family B", "FAMB001");
        var deviceIdB = Guid.NewGuid();
        var userB = await CreateTestUserAsync("userB", "Password123", familyB.FamilyId, deviceIdB);

        // Authenticate as userB
        var tokenB = TestJwtHelper.GenerateTestToken(userB.Id, userB.Username, deviceIdB, familyB.FamilyId);
        SetAuthorizationHeader(tokenB);

        // Try to update Family A's product
        var updateDto = new ProductUpsertDto
        {
            Id = productA.Id,
            ListId = listA.Id,
            Name = "Hacked Milk",
            Quantity = "999",
            Status = ProductStatus.Paid,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/products/v1/update", updateDto);

        // Assert
        // Should fail with either 401 Unauthorized or 404 NotFound (to not leak info about other families)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);

        // Verify the product was NOT modified
        var unchangedProduct = await _dbContext.Products.FindAsync(productA.Id);
        unchangedProduct!.Name.Should().Be("Milk");
        unchangedProduct.Quantity.Should().Be("1");
    }

    [Fact]
    public async Task SyncBatch_WithCrossFamilyListId_IgnoresInvalidProducts()
    {
        // Arrange
        // Create Family A with a list
        var familyA = await CreateTestFamilyAsync("Family A", "FAMA001");
        var listA = await CreateTestShoppingListAsync(familyA.FamilyId);
        var deviceIdA = Guid.NewGuid();
        var userA = await CreateTestUserAsync("userA", "Password123", familyA.FamilyId, deviceIdA);

        // Create Family B with a list
        var familyB = await CreateTestFamilyAsync("Family B", "FAMB001");
        var listB = await CreateTestShoppingListAsync(familyB.FamilyId);

        // Authenticate as userA
        var tokenA = TestJwtHelper.GenerateTestToken(userA.Id, userA.Username, deviceIdA, familyA.FamilyId);
        SetAuthorizationHeader(tokenA);

        // Try to sync products: 2 valid for Family A, 1 for Family B's list
        var syncBatch = new SyncBatchDto
        {
            Products = new List<ProductUpsertDto>
            {
                new ProductUpsertDto
                {
                    Id = Guid.NewGuid(),
                    ListId = listA.Id, // Valid for Family A
                    Name = "Valid Product 1",
                    Quantity = "1",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = DateTime.UtcNow
                },
                new ProductUpsertDto
                {
                    Id = Guid.NewGuid(),
                    ListId = listA.Id, // Valid for Family A
                    Name = "Valid Product 2",
                    Quantity = "2",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = DateTime.UtcNow
                },
                new ProductUpsertDto
                {
                    Id = Guid.NewGuid(),
                    ListId = listB.Id, // INVALID - belongs to Family B
                    Name = "Invalid Product",
                    Quantity = "3",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = DateTime.UtcNow
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/v1/sync", syncBatch);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var syncResult = await response.Content.ReadFromJsonAsync<SyncResultDto>();
        syncResult.Should().NotBeNull();
        syncResult!.TotalProcessed.Should().Be(3);
        syncResult.Synced.Should().HaveCount(2); // Only the 2 valid products
        syncResult.Ignored.Should().HaveCount(1); // The cross-family product
    }

    #endregion

    #region Last-Write-Wins (LWW) Conflict Resolution Tests

    [Fact]
    public async Task UpdateProduct_OldTimestamp_IgnoresUpdate()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var list = await CreateTestShoppingListAsync(family.FamilyId);
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);

        // Create a product with timestamp at 10:05 AM
        var currentTimestamp = new DateTime(2026, 2, 17, 10, 5, 0, DateTimeKind.Utc);
        var product = await CreateTestProductAsync(
            list.Id, 
            "Original Name", 
            user.Id, 
            quantity: 5,
            clientTimestamp: currentTimestamp);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Try to update with an older timestamp (10:00 AM - 5 minutes before)
        var staleTimestamp = new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc);
        var updateDto = new ProductUpsertDto
        {
            Id = product.Id,
            ListId = list.Id,
            Name = "Stale Update",
            Quantity = "999",
            Status = ProductStatus.Paid,
            ClientTimestamp = staleTimestamp // Older than DB
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/products/v1/update", updateDto);

        // Assert
        // Should return 200 OK or 409 Conflict depending on your implementation
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);

        // The critical assertion: product should NOT be updated
        var unchangedProduct = await _dbContext.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        unchangedProduct.Name.Should().Be("Original Name");
        unchangedProduct.Quantity.Should().Be("5");
    }

    [Fact]
    public async Task UpdateProduct_NewerTimestamp_AppliesUpdate()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var list = await CreateTestShoppingListAsync(family.FamilyId);
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);

        // Create a product with timestamp at 10:05 AM
        var currentTimestamp = new DateTime(2026, 2, 17, 10, 5, 0, DateTimeKind.Utc);
        var product = await CreateTestProductAsync(
            list.Id, 
            "Original Name", 
            user.Id, 
            quantity: 5,
            clientTimestamp: currentTimestamp);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Update with a newer timestamp (10:10 AM - 5 minutes after)
        var newerTimestamp = new DateTime(2026, 2, 17, 10, 10, 0, DateTimeKind.Utc);
        var updateDto = new ProductUpsertDto
        {
            Id = product.Id,
            ListId = list.Id,
            Name = "Updated Name",
            Quantity = "10",
            Status = ProductStatus.Paid,
            ClientTimestamp = newerTimestamp // Newer than DB
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/products/v1/update", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the product WAS updated
        var updatedProduct = await _dbContext.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        updatedProduct.Name.Should().Be("Updated Name");
        updatedProduct.Quantity.Should().Be("10");
        updatedProduct.Status.Should().Be(ProductStatus.Paid);
    }

    [Fact]
    public async Task SyncBatch_LWWAppliedToEachProduct()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var list = await CreateTestShoppingListAsync(family.FamilyId);
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);

        // Create existing products with known timestamps
        var baseTime = new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc);
        var product1 = await CreateTestProductAsync(list.Id, "Product 1", user.Id, clientTimestamp: baseTime);
        var product2 = await CreateTestProductAsync(list.Id, "Product 2", user.Id, clientTimestamp: baseTime);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Sync batch with mixed timestamps
        var syncBatch = new SyncBatchDto
        {
            Products = new List<ProductUpsertDto>
            {
                new ProductUpsertDto
                {
                    Id = product1.Id,
                    ListId = list.Id,
                    Name = "Product 1 Updated",
                    Quantity = "99",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = baseTime.AddMinutes(10) // NEWER - should apply
                },
                new ProductUpsertDto
                {
                    Id = product2.Id,
                    ListId = list.Id,
                    Name = "Product 2 Stale",
                    Quantity = "88",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = baseTime.AddMinutes(-10) // OLDER - should ignore
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/v1/sync", syncBatch);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify Product 1 was updated
        var updated1 = await _dbContext.Products.AsNoTracking().FirstAsync(p => p.Id == product1.Id);
        updated1.Name.Should().Be("Product 1 Updated");
        updated1.Quantity.Should().Be("99");

        // Verify Product 2 was NOT updated
        var updated2 = await _dbContext.Products.AsNoTracking().FirstAsync(p => p.Id == product2.Id);
        updated2.Name.Should().Be("Product 2");
        updated2.Quantity.Should().Be("1"); // Original value
    }

    #endregion

    #region Batch Sync Atomic Integrity Tests

    [Fact]
    public async Task SyncBatch_PartialSuccess_ValidProductsSavedInvalidReported()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var list = await CreateTestShoppingListAsync(family.FamilyId);
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Create a batch with 5 valid products and 1 with invalid ListId
        var invalidListId = Guid.NewGuid(); // Non-existent list
        var syncBatch = new SyncBatchDto
        {
            Products = new List<ProductUpsertDto>
            {
                new ProductUpsertDto { Id = Guid.NewGuid(), ListId = list.Id, Name = "Valid 1", Quantity = "1", Status = ProductStatus.Pending, ClientTimestamp = DateTime.UtcNow },
                new ProductUpsertDto { Id = Guid.NewGuid(), ListId = list.Id, Name = "Valid 2", Quantity = "1", Status = ProductStatus.Pending, ClientTimestamp = DateTime.UtcNow },
                new ProductUpsertDto { Id = Guid.NewGuid(), ListId = list.Id, Name = "Valid 3", Quantity = "1", Status = ProductStatus.Pending, ClientTimestamp = DateTime.UtcNow },
                new ProductUpsertDto { Id = Guid.NewGuid(), ListId = invalidListId, Name = "Invalid", Quantity = "1", Status = ProductStatus.Pending, ClientTimestamp = DateTime.UtcNow }, // INVALID
                new ProductUpsertDto { Id = Guid.NewGuid(), ListId = list.Id, Name = "Valid 4", Quantity = "1", Status = ProductStatus.Pending, ClientTimestamp = DateTime.UtcNow },
                new ProductUpsertDto { Id = Guid.NewGuid(), ListId = list.Id, Name = "Valid 5", Quantity = "1", Status = ProductStatus.Pending, ClientTimestamp = DateTime.UtcNow },
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/v1/sync", syncBatch);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var syncResult = await response.Content.ReadFromJsonAsync<SyncResultDto>();
        syncResult.Should().NotBeNull();
        syncResult!.TotalProcessed.Should().Be(6);
        syncResult.Synced.Should().HaveCount(5); // 5 valid products
        syncResult.Ignored.Should().HaveCount(1); // 1 invalid product

        // Verify the 5 valid products exist in the database
        var savedProducts = await _dbContext.Products.Where(p => p.ListId == list.Id).ToListAsync();
        savedProducts.Should().HaveCount(5);
        savedProducts.Should().Contain(p => p.Name == "Valid 1");
        savedProducts.Should().Contain(p => p.Name == "Valid 5");
        savedProducts.Should().NotContain(p => p.Name == "Invalid");
    }

    #endregion

    #region Idempotent Deletion Tests

    [Fact]
    public async Task DeleteProduct_AlreadyDeleted_ReturnsOk()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var list = await CreateTestShoppingListAsync(family.FamilyId);
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var product = await CreateTestProductAsync(list.Id, "Test Product", user.Id);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Delete the product for the first time
        var firstDelete = await _client.DeleteAsync($"/api/products/v1/delete/{product.Id}");
        firstDelete.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Delete the same product again
        var secondDelete = await _client.DeleteAsync($"/api/products/v1/delete/{product.Id}");

        // Assert - Should return 200 OK to maintain idempotency
        // This prevents mobile app sync loops from crashing
        secondDelete.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        // Either is acceptable for idempotent deletion
    }

    [Fact]
    public async Task DeleteProduct_NonExistent_DoesNotCrash()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/products/v1/delete/{nonExistentId}");

        // Assert
        // Should not crash with 500, should return 404 or 200
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Product Creation and Retrieval Tests

    [Fact]
    public async Task GetProductsByListId_WithValidFamilyList_ReturnsOnlyListProducts()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var listA = await CreateTestShoppingListAsync(family.FamilyId, "List A");
        var listB = await CreateTestShoppingListAsync(family.FamilyId, "List B");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);

        var productA1 = await CreateTestProductAsync(listA.Id, "Milk", user.Id);
        var productA2 = await CreateTestProductAsync(listA.Id, "Bread", user.Id);
        await CreateTestProductAsync(listB.Id, "Eggs", user.Id);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync($"/api/products/v1/list/{listA.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        products.Should().NotBeNull();
        products.Should().HaveCount(2);
        products.Should().Contain(p => p.Id == productA1.Id);
        products.Should().Contain(p => p.Id == productA2.Id);
        products.Should().OnlyContain(p => p.ListId == listA.Id);
    }

    [Fact]
    public async Task GetProductsByListId_FromDifferentFamilyList_ReturnsUnauthorized()
    {
        // Arrange
        var familyA = await CreateTestFamilyAsync("Family A", "FAMA001");
        var listA = await CreateTestShoppingListAsync(familyA.FamilyId);
        var deviceIdA = Guid.NewGuid();
        var userA = await CreateTestUserAsync("userA", "Password123", familyA.FamilyId, deviceIdA);
        await CreateTestProductAsync(listA.Id, "Milk", userA.Id);

        var familyB = await CreateTestFamilyAsync("Family B", "FAMB001");
        var deviceIdB = Guid.NewGuid();
        var userB = await CreateTestUserAsync("userB", "Password123", familyB.FamilyId, deviceIdB);

        var tokenB = TestJwtHelper.GenerateTestToken(userB.Id, userB.Username, deviceIdB, familyB.FamilyId);
        SetAuthorizationHeader(tokenB);

        // Act
        var response = await _client.GetAsync($"/api/products/v1/list/{listA.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsOk()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "TEST001");
        var list = await CreateTestShoppingListAsync(family.FamilyId);
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);

        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var productDto = new ProductUpsertDto
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            Name = "New Product",
            Quantity = "3",
            Status = ProductStatus.Pending,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/v1/create", productDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var productResponse = await response.Content.ReadFromJsonAsync<ProductResponse>();
        productResponse.Should().NotBeNull();
        productResponse!.Name.Should().Be("New Product");
        productResponse.Quantity.Should().Be("3");

        // Verify in database
        var savedProduct = await _dbContext.Products.FindAsync(productDto.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("New Product");
    }

    #endregion
}

