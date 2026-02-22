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
/// Integration tests for the Shopping Lists Controller.
/// Tests list creation, retrieval, soft delete, restore, and security validations.
/// </summary>
public class ShoppingListsControllerIntegrationTests : IntegrationTestBase
{
    public ShoppingListsControllerIntegrationTests(PostgresTestcontainerFixture fixture) : base(fixture)
    {
    }

    #region Create Shopping List Tests

    [Fact]
    public async Task CreateShoppingList_WithValidData_ReturnsOkWithListInfo()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var request = new CreateShoppingListRequest
        {
            Name = "Weekly Groceries"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/shopping-lists/v1/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var listResponse = await response.Content.ReadFromJsonAsync<ShoppingListResponse>();
        listResponse.Should().NotBeNull();
        listResponse!.Name.Should().Be("Weekly Groceries");
        listResponse.FamilyId.Should().Be(family.FamilyId);
        listResponse.IsActive.Should().BeTrue();
        listResponse.Id.Should().NotBeEmpty();

        // Verify list exists in database
        ClearChangeTracker();
        var dbList = await _dbContext.ShoppingLists.FirstOrDefaultAsync(sl => sl.Id == listResponse.Id);
        dbList.Should().NotBeNull();
        dbList!.Name.Should().Be("Weekly Groceries");
        dbList.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateShoppingList_WhenUserHasNoFamily_ReturnsBadRequest()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        var request = new CreateShoppingListRequest
        {
            Name = "Weekly Groceries"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/shopping-lists/v1/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("must belong to a family");
    }

    [Fact]
    public async Task CreateShoppingList_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateShoppingListRequest
        {
            Name = "Weekly Groceries"
        };

        // Act (no authorization header set)
        var response = await _client.PostAsJsonAsync("/api/shopping-lists/v1/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Shopping Lists Tests

    [Fact]
    public async Task GetFamilyShoppingLists_ReturnsOnlyActiveListsByDefault()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Create both active and inactive lists
        var activeList = await CreateTestShoppingListAsync(family.FamilyId, "Active List");
        var inactiveList = await CreateTestShoppingListAsync(family.FamilyId, "Inactive List");
        inactiveList.IsActive = false;
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/shopping-lists/v1/list");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var lists = await response.Content.ReadFromJsonAsync<IEnumerable<ShoppingListResponse>>();
        lists.Should().NotBeNull();
        lists!.Should().HaveCount(1);
        lists!.First().Name.Should().Be("Active List");
        lists!.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetFamilyShoppingLists_WithIncludeInactive_ReturnsAllLists()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Create both active and inactive lists
        var activeList = await CreateTestShoppingListAsync(family.FamilyId, "Active List");
        var inactiveList = await CreateTestShoppingListAsync(family.FamilyId, "Inactive List");
        inactiveList.IsActive = false;
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/shopping-lists/v1/list?includeInactive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var lists = await response.Content.ReadFromJsonAsync<IEnumerable<ShoppingListResponse>>();
        lists.Should().NotBeNull();
        lists!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFamilyShoppingLists_OnlyReturnsOwnFamilyLists()
    {
        // Arrange - Security test: ensure users can't see other families' lists
        var family1 = await CreateTestFamilyAsync("Family 1", "FAM001");
        var family2 = await CreateTestFamilyAsync("Family 2", "FAM002");
        
        var deviceId = Guid.NewGuid();
        var user1 = await CreateTestUserAsync("user1", "Password123", family1.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user1.Id, user1.Username, deviceId, family1.FamilyId);
        SetAuthorizationHeader(token);

        // Create lists for both families
        await CreateTestShoppingListAsync(family1.FamilyId, "Family 1 List");
        await CreateTestShoppingListAsync(family2.FamilyId, "Family 2 List");

        // Act
        var response = await _client.GetAsync("/api/shopping-lists/v1/list");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var lists = await response.Content.ReadFromJsonAsync<IEnumerable<ShoppingListResponse>>();
        lists.Should().NotBeNull();
        lists!.Should().HaveCount(1);
        lists!.First().Name.Should().Be("Family 1 List");
        lists!.First().FamilyId.Should().Be(family1.FamilyId);
    }

    [Fact]
    public async Task GetFamilyShoppingLists_WhenUserHasNoFamily_ReturnsBadRequest()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/shopping-lists/v1/list");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("must belong to a family");
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public async Task SoftDeleteShoppingList_WithValidList_SetsIsActiveToFalse()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Test List");

        // Act
        var response = await _client.DeleteAsync($"/api/shopping-lists/v1/{list.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify list is soft deleted, not hard deleted
        ClearChangeTracker();
        var dbList = await _dbContext.ShoppingLists.FirstOrDefaultAsync(sl => sl.Id == list.Id);
        dbList.Should().NotBeNull(); // Still exists
        dbList!.IsActive.Should().BeFalse(); // But marked inactive
    }

    [Fact]
    public async Task SoftDeleteShoppingList_WithNonexistentList_ReturnsNotFound()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var nonexistentListId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/shopping-lists/v1/{nonexistentListId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SoftDeleteShoppingList_WithOtherFamilyList_ReturnsUnauthorized()
    {
        // Arrange - Security test: user tries to delete another family's list
        var family1 = await CreateTestFamilyAsync("Family 1", "FAM001");
        var family2 = await CreateTestFamilyAsync("Family 2", "FAM002");
        
        var deviceId = Guid.NewGuid();
        var user1 = await CreateTestUserAsync("user1", "Password123", family1.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user1.Id, user1.Username, deviceId, family1.FamilyId);
        SetAuthorizationHeader(token);

        // Create list for family 2
        var family2List = await CreateTestShoppingListAsync(family2.FamilyId, "Family 2 List");

        // Act - User 1 tries to delete Family 2's list
        var response = await _client.DeleteAsync($"/api/shopping-lists/v1/{family2List.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Verify the list was NOT deleted
        ClearChangeTracker();
        var dbList = await _dbContext.ShoppingLists.FirstOrDefaultAsync(sl => sl.Id == family2List.Id);
        dbList.Should().NotBeNull();
        dbList!.IsActive.Should().BeTrue(); // Still active
    }

    [Fact]
    public async Task SoftDeleteShoppingList_PreservesProductsForHistoricalData()
    {
        // Arrange - Verify that products are preserved when list is soft deleted
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Shopping List");
        await CreateTestProductAsync(list.Id, "Milk", user.Id);
        await CreateTestProductAsync(list.Id, "Bread", user.Id);

        // Act
        var response = await _client.DeleteAsync($"/api/shopping-lists/v1/{list.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify products still exist (for historical data)
        ClearChangeTracker();
        var products = await _dbContext.Products.Where(p => p.ListId == list.Id).ToListAsync();
        products.Should().HaveCount(2); // Products preserved
    }

    #endregion

    #region Restore Tests

    [Fact]
    public async Task RestoreShoppingList_WithValidList_SetsIsActiveToTrue()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Deleted List");
        list.IsActive = false;
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/shopping-lists/v1/{list.Id}/restore", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify list is restored
        ClearChangeTracker();
        var dbList = await _dbContext.ShoppingLists.FirstOrDefaultAsync(sl => sl.Id == list.Id);
        dbList.Should().NotBeNull();
        dbList!.IsActive.Should().BeTrue(); // Restored
    }

    [Fact]
    public async Task RestoreShoppingList_WithOtherFamilyList_ReturnsUnauthorized()
    {
        // Arrange - Security test
        var family1 = await CreateTestFamilyAsync("Family 1", "FAM001");
        var family2 = await CreateTestFamilyAsync("Family 2", "FAM002");
        
        var deviceId = Guid.NewGuid();
        var user1 = await CreateTestUserAsync("user1", "Password123", family1.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user1.Id, user1.Username, deviceId, family1.FamilyId);
        SetAuthorizationHeader(token);

        var family2List = await CreateTestShoppingListAsync(family2.FamilyId, "Family 2 List");
        family2List.IsActive = false;
        await _dbContext.SaveChangesAsync();

        // Act - User 1 tries to restore Family 2's list
        var response = await _client.PostAsync($"/api/shopping-lists/v1/{family2List.Id}/restore", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RestoreShoppingList_WithNonexistentList_ReturnsNotFound()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var nonexistentListId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/shopping-lists/v1/{nonexistentListId}/restore", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update Status Tests

    [Fact]
    public async Task UpdateShoppingListStatus_WithValidList_UpdatesStatus()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "TEST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var list = await CreateTestShoppingListAsync(family.FamilyId, "Test List");

        // Act
        var response = await _client.PatchAsync($"/api/shopping-lists/v1/{list.Id}/status?isActive=false", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ClearChangeTracker();
        var dbList = await _dbContext.ShoppingLists.FirstOrDefaultAsync(sl => sl.Id == list.Id);
        dbList!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateShoppingListStatus_WithOtherFamilyList_ReturnsUnauthorized()
    {
        // Arrange - Security test
        var family1 = await CreateTestFamilyAsync("Family 1", "FAM001");
        var family2 = await CreateTestFamilyAsync("Family 2", "FAM002");
        
        var deviceId = Guid.NewGuid();
        var user1 = await CreateTestUserAsync("user1", "Password123", family1.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user1.Id, user1.Username, deviceId, family1.FamilyId);
        SetAuthorizationHeader(token);

        var family2List = await CreateTestShoppingListAsync(family2.FamilyId, "Family 2 List");

        // Act
        var response = await _client.PatchAsync($"/api/shopping-lists/v1/{family2List.Id}/status?isActive=false", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
