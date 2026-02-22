using System.Net;
using System.Net.Http.Json;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.Enums;
using BackSharedGroceries.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Tests.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the auto-deactivation feature.
/// Tests that shopping lists are automatically deactivated when all products are marked as Paid.
/// </summary>
public class ShoppingListAutoDeactivationIntegrationTests : IntegrationTestBase
{
    public ShoppingListAutoDeactivationIntegrationTests(PostgresTestcontainerFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AddProduct_WhenAllProductsBecomePaid_AutoDeactivatesList()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Shopping List");
        
        // Add initial products - all paid except one
        await CreateTestProductAsync(list.Id, "Milk", user.Id, status: ProductStatus.Paid);
        await CreateTestProductAsync(list.Id, "Bread", user.Id, status: ProductStatus.Paid);
        await CreateTestProductAsync(list.Id, "Eggs", user.Id, status: ProductStatus.InCart);

        // Verify list is still active
        list.IsActive.Should().BeTrue();

        // Add the last product as Paid - this should trigger auto-deactivation
        var newProduct = new ProductUpsertDto
        {
            Id = Guid.NewGuid(),
            Name = "Butter",
            Quantity = "1",
            Status = ProductStatus.Paid,
            ListId = list.Id,
            ClientTimestamp = DateTime.UtcNow
        };

        // Update the "Eggs" product to Paid
        var eggsProduct = await _dbContext.Products.FirstAsync(p => p.Name == "Eggs");
        var updateDto = new ProductUpsertDto
        {
            Id = eggsProduct.Id,
            Name = "Eggs",
            Quantity = "12",
            Status = ProductStatus.Paid,
            ListId = list.Id,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/products/v1/update/{eggsProduct.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify that the list was auto-deactivated
        ClearChangeTracker();
        var updatedList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        updatedList.IsActive.Should().BeFalse("because all products are now paid");
    }

    [Fact]
    public async Task UpdateProduct_ChangingStatusToPaid_WhenAllPaid_AutoDeactivatesList()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Shopping List");
        
        // Create products - all paid except one pending
        await CreateTestProductAsync(list.Id, "Milk", user.Id, status: ProductStatus.Paid);
        var breadProduct = await CreateTestProductAsync(list.Id, "Bread", user.Id, status: ProductStatus.Pending);

        // Verify list is still active
        ClearChangeTracker();
        var activeList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        activeList.IsActive.Should().BeTrue();

        // Update the pending product to Paid
        var updateDto = new ProductUpsertDto
        {
            Id = breadProduct.Id,
            Name = "Bread",
            Quantity = "1",
            Status = ProductStatus.Paid,
            ListId = list.Id,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/products/v1/update/{breadProduct.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify that the list was auto-deactivated
        ClearChangeTracker();
        var updatedList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        updatedList.IsActive.Should().BeFalse("because all products are now paid");
    }

    [Fact]
    public async Task UpdateProduct_WhenNotAllProductsPaid_ListRemainsActive()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Shopping List");
        
        // Create products with mixed statuses
        var milkProduct = await CreateTestProductAsync(list.Id, "Milk", user.Id, status: ProductStatus.Pending);
        await CreateTestProductAsync(list.Id, "Bread", user.Id, status: ProductStatus.Pending);

        // Update one product to Paid (but not all)
        var updateDto = new ProductUpsertDto
        {
            Id = milkProduct.Id,
            Name = "Milk",
            Quantity = "1L",
            Status = ProductStatus.Paid,
            ListId = list.Id,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/products/v1/update/{milkProduct.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify that the list is still active (not all products paid)
        ClearChangeTracker();
        var updatedList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        updatedList.IsActive.Should().BeTrue("because not all products are paid yet");
    }

    [Fact]
    public async Task SyncBatch_WhenAllProductsBecomePaid_AutoDeactivatesList()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Shopping List");
        
        // Create initial pending products
        var product1 = await CreateTestProductAsync(list.Id, "Milk", user.Id, status: ProductStatus.Pending);
        var product2 = await CreateTestProductAsync(list.Id, "Bread", user.Id, status: ProductStatus.Pending);

        // Create batch to sync all products as Paid
        var batch = new SyncBatchDto
        {
            Products = new List<ProductUpsertDto>
            {
                new ProductUpsertDto
                {
                    Id = product1.Id,
                    Name = "Milk",
                    Quantity = "1L",
                    Status = ProductStatus.Paid,
                    ListId = list.Id,
                    ClientTimestamp = DateTime.UtcNow
                },
                new ProductUpsertDto
                {
                    Id = product2.Id,
                    Name = "Bread",
                    Quantity = "1",
                    Status = ProductStatus.Paid,
                    ListId = list.Id,
                    ClientTimestamp = DateTime.UtcNow
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/v1/sync", batch);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify that the list was auto-deactivated
        ClearChangeTracker();
        var updatedList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        updatedList.IsActive.Should().BeFalse("because all products were synced as paid");
    }

    [Fact]
    public async Task EmptyList_IsConsideredAllPaid_AutoDeactivates()
    {
        // Arrange - Edge case: empty list should be considered "all paid"
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Empty List");
        var existingProduct = await CreateTestProductAsync(list.Id, "Milk", user.Id, status: ProductStatus.Paid);

        // Delete the only product, leaving the list empty
        var deleteResponse = await _client.DeleteAsync($"/api/products/v1/delete/{existingProduct.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Now add a new product as Paid - the list has no other products, so all are paid
        var newProduct = new ProductUpsertDto
        {
            Id = Guid.NewGuid(),
            Name = "Bread",
            Quantity = "1",
            Status = ProductStatus.Paid,
            ListId = list.Id,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/v1/create", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the list was auto-deactivated (single product, and it's paid)
        ClearChangeTracker();
        var updatedList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        updatedList.IsActive.Should().BeFalse("because the only product is paid");
    }

    [Fact]
    public async Task RestoreList_AfterAutoDeactivation_CanBeReactivated()
    {
        // Arrange - Test that auto-deactivated lists can still be manually restored
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Shopping List");
        
        var product = await CreateTestProductAsync(list.Id, "Milk", user.Id, status: ProductStatus.Pending);

        // Update product to Paid to trigger auto-deactivation
        var updateDto = new ProductUpsertDto
        {
            Id = product.Id,
            Name = "Milk",
            Quantity = "1L",
            Status = ProductStatus.Paid,
            ListId = list.Id,
            ClientTimestamp = DateTime.UtcNow
        };

        var updateResponse = await _client.PatchAsJsonAsync($"/api/products/v1/update/{product.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify list is deactivated
        ClearChangeTracker();
        var deactivatedList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        deactivatedList.IsActive.Should().BeFalse();

        // Act - Restore the list
        var restoreResponse = await _client.PostAsync($"/api/shopping-lists/v1/{list.Id}/restore", null);

        // Assert
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearChangeTracker();
        var restoredList = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list.Id);
        restoredList.IsActive.Should().BeTrue("because it was manually restored");
    }

    [Fact]
    public async Task MultipleListsInFamily_OnlyAffectedListIsDeactivated()
    {
        // Arrange - Test that only the list with all paid products is deactivated
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list1 = await CreateTestShoppingListAsync(family.FamilyId, "List 1");
        var list2 = await CreateTestShoppingListAsync(family.FamilyId, "List 2");
        
        // List 1: will have all products paid
        var list1Product = await CreateTestProductAsync(list1.Id, "Milk", user.Id, status: ProductStatus.Pending);
        
        // List 2: will have pending products
        await CreateTestProductAsync(list2.Id, "Bread", user.Id, status: ProductStatus.Pending);

        // Update List 1 product to Paid
        var updateDto = new ProductUpsertDto
        {
            Id = list1Product.Id,
            Name = "Milk",
            Quantity = "1L",
            Status = ProductStatus.Paid,
            ListId = list1.Id,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/products/v1/update/{list1Product.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearChangeTracker();
        var updatedList1 = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list1.Id);
        var updatedList2 = await _dbContext.ShoppingLists.FirstAsync(sl => sl.Id == list2.Id);

        updatedList1.IsActive.Should().BeFalse("because all products in List 1 are paid");
        updatedList2.IsActive.Should().BeTrue("because List 2 still has pending products");
    }
}
