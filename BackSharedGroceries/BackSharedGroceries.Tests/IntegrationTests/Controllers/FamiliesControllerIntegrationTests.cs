using System.Net;
using System.Net.Http.Json;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Tests.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the Families Controller.
/// Tests family creation, joining, leaving, and edge cases like last member leaving.
/// </summary>
public class FamiliesControllerIntegrationTests : IntegrationTestBase
{
    public FamiliesControllerIntegrationTests(PostgresTestcontainerFixture fixture) : base(fixture)
    {
    }

    #region Create Family Tests

    [Fact]
    public async Task CreateFamily_WithValidData_ReturnsOkWithInviteCode()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        var request = new CreateFamilyRequest
        {
            FamilyName = "Smith Family"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/families/v1/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var familyResponse = await response.Content.ReadFromJsonAsync<FamilyResponse>();
        familyResponse.Should().NotBeNull();
        familyResponse!.Name.Should().Be("Smith Family");
        familyResponse.InviteCode.Should().NotBeNullOrEmpty();

        // Verify user is assigned to the family
        ClearChangeTracker(); // Clear cache to get fresh data from DB
        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser!.FamilyId.Should().NotBeNull();
        updatedUser.FamilyId.Should().Be(familyResponse.Id);
    }

    [Fact]
    public async Task CreateFamily_WhenUserAlreadyHasFamily_ReturnsBadRequest()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Existing Family", "EXIST123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        var request = new CreateFamilyRequest
        {
            FamilyName = "New Family"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/families/v1/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already"); // User already belongs to a family
    }

    [Fact]
    public async Task CreateFamily_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateFamilyRequest
        {
            FamilyName = "Smith Family"
        };

        // Act (no authorization header set)
        var response = await _client.PostAsJsonAsync("/api/families/v1/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Join Family Tests

    [Fact]
    public async Task JoinFamily_WithValidCode_ReturnsOk()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Existing Family", "ABC123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        var request = new JoinFamilyRequest
        {
            InviteCode = "ABC123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/families/v1/join", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify user is now part of the family
        ClearChangeTracker(); // Clear cache to get fresh data from DB
        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser!.FamilyId.Should().Be(family.FamilyId);
    }

    [Fact]
    public async Task JoinFamily_WithNormalizedCode_ReturnsOk()
    {
        // Arrange
        // Family code in DB is "ABC123" (uppercase)
        var family = await CreateTestFamilyAsync("Existing Family", "ABC123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        // User tries to join with lowercase and hyphen: "abc-123"
        var request = new JoinFamilyRequest
        {
            InviteCode = "abc-123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/families/v1/join", request);

        // Assert
        // Should succeed because of normalization (removing hyphens, converting to uppercase)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ClearChangeTracker(); // Clear cache to get fresh data from DB
        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser!.FamilyId.Should().Be(family.FamilyId);
    }

    [Fact]
    public async Task JoinFamily_WithNonExistentCode_ReturnsNotFound()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        var request = new JoinFamilyRequest
        {
            InviteCode = "INVALID999"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/families/v1/join", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task JoinFamily_WhenUserAlreadyHasFamily_ReturnsBadRequest()
    {
        // Arrange
        var family1 = await CreateTestFamilyAsync("Family 1", "FAM001");
        var family2 = await CreateTestFamilyAsync("Family 2", "FAM002");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", family1.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family1.FamilyId);
        SetAuthorizationHeader(token);

        var request = new JoinFamilyRequest
        {
            InviteCode = "FAM002"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/families/v1/join", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Leave Family Tests

    [Fact]
    public async Task LeaveFamily_WhenLastMember_DeletesFamily()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Solo Family", "SOLO123");
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("onlymember", "Password123", family.FamilyId, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId, family.FamilyId);
        SetAuthorizationHeader(token);

        // Verify family exists before leaving
        var familyBefore = await _dbContext.Families.FindAsync(family.FamilyId);
        familyBefore.Should().NotBeNull();

        // Act
        var response = await _client.PostAsync("/api/families/v1/leave", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify user is no longer part of the family
        ClearChangeTracker(); // Clear cache to get fresh data from DB
        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser!.FamilyId.Should().BeNull();

        // Verify family was deleted from the database
        var familyAfter = await _dbContext.Families.FindAsync(family.FamilyId);
        familyAfter.Should().BeNull();
    }

    [Fact]
    public async Task LeaveFamily_WhenMultipleMembers_OnlyRemovesUser()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Multi-Member Family", "MULTI123");
        var deviceId1 = Guid.NewGuid();
        var deviceId2 = Guid.NewGuid();
        var user1 = await CreateTestUserAsync("member1", "Password123", family.FamilyId, deviceId1);
        var user2 = await CreateTestUserAsync("member2", "Password123", family.FamilyId, deviceId2);
        
        var token1 = TestJwtHelper.GenerateTestToken(user1.Id, user1.Username, deviceId1, family.FamilyId);
        SetAuthorizationHeader(token1);

        // Act
        var response = await _client.PostAsync("/api/families/v1/leave", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify user1 is no longer part of the family
        ClearChangeTracker(); // Clear cache to get fresh data from DB
        var updatedUser1 = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user1.Id);
        updatedUser1!.FamilyId.Should().BeNull();

        // Verify user2 is still part of the family
        var updatedUser2 = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user2.Id);
        updatedUser2!.FamilyId.Should().Be(family.FamilyId);

        // Verify family still exists
        var familyAfter = await _dbContext.Families.FindAsync(family.FamilyId);
        familyAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task LeaveFamily_WhenUserNotInFamily_ReturnsBadRequest()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("noFamilyUser", "Password123", null, deviceId);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, deviceId);
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.PostAsync("/api/families/v1/leave", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("not part of");
    }

    #endregion
}
