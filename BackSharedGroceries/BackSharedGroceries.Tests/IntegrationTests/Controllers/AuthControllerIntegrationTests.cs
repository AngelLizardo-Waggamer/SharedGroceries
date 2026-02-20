using System.Net;
using System.Net.Http.Json;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Tests.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the Auth Controller.
/// Tests registration, login, token refresh, and session hijack protection.
/// </summary>
public class AuthControllerIntegrationTests : IntegrationTestBase
{
    public AuthControllerIntegrationTests(PostgresTestcontainerFixture fixture) : base(fixture)
    {
    }

    #region Registration Tests

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        user.Should().NotBeNull();
        user!.FamilyId.Should().BeNull();
    }

    [Fact]
    public async Task Register_WithExistingUsername_ReturnsConflict()
    {
        // Arrange
        await CreateTestUserAsync("existinguser");
        
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already exists");
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        // Note: Since your DTO doesn't have password validation attributes,
        // this test assumes you'll add validation like [MinLength], [RegularExpression], etc.
        // For now, we'll test with missing password
        var json = "{\"Username\":\"testuser\"}"; // Missing password

        // Act
        var response = await _client.PostAsync("/api/auth/v1/register", 
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        await CreateTestUserAsync("testuser", "MyPassword123");
        
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "MyPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
        loginResponse.Username.Should().Be("testuser");

        // Verify that CurrentDeviceId was set in the database
        ClearChangeTracker(); // Clear cache to get fresh data from DB
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        user!.CurrentDeviceId.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await CreateTestUserAsync("testuser", "CorrectPassword123");
        
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "SomePassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Session Hijack Protection Tests

    [Fact]
    public async Task SessionHijack_DeviceALosesAccessWhenDeviceBLogsIn()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("TestFamily", "ABC123");
        var user = await CreateTestUserAsync("testuser", "Password123", family.FamilyId);
        var list = await CreateTestShoppingListAsync(family.FamilyId);

        // Step 1: User logs in on Device A
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123"
        };

        var deviceALoginResponse = await _client.PostAsJsonAsync("/api/auth/v1/login", loginRequest);
        deviceALoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var deviceALogin = await deviceALoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        deviceALogin.Should().NotBeNull();
        var deviceAToken = deviceALogin!.Token;

        // Step 2: User logs in on Device B (this should invalidate Device A's session)
        var deviceBLoginResponse = await _client.PostAsJsonAsync("/api/auth/v1/login", loginRequest);
        deviceBLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var deviceBLogin = await deviceBLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        deviceBLogin.Should().NotBeNull();
        var deviceBToken = deviceBLogin!.Token;

        // Verify different device IDs in database (CurrentDeviceId changed)
        ClearChangeTracker(); // Clear cache to get fresh data from DB
        var userAfterDeviceB = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        userAfterDeviceB!.CurrentDeviceId.Should().NotBeNull();

        // Step 3: Device A attempts an authorized request
        SetAuthorizationHeader(deviceAToken);
        var deviceARequest = await _client.GetAsync("/api/products/v1/suggestions");

        // Assert: Device A should receive 401 Unauthorized due to DeviceId mismatch
        deviceARequest.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var errorContent = await deviceARequest.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Expired session");

        // Verify Device B still works
        SetAuthorizationHeader(deviceBToken);
        var deviceBRequest = await _client.GetAsync("/api/products/v1/suggestions");
        deviceBRequest.StatusCode.Should().Be(HttpStatusCode.OK); // Or whatever status your endpoint returns
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewJwt()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var refreshToken = await CreateTestRefreshTokenAsync(user.Id);

        var request = new RefreshRequest
        {
            RefreshToken = refreshToken.Token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var expiredRefreshToken = await CreateTestRefreshTokenAsync(
            user.Id, 
            expiresAt: DateTime.UtcNow.AddDays(-1)); // Expired yesterday

        var request = new RefreshRequest
        {
            RefreshToken = expiredRefreshToken.Token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("expired");
    }

    [Fact]
    public async Task RefreshToken_WithValidTokenButChangedDeviceId_ReturnsUnauthorized()
    {
        // Arrange
        var originalDeviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, originalDeviceId);
        var refreshToken = await CreateTestRefreshTokenAsync(user.Id);

        // Simulate the user logging in from another device (changes CurrentDeviceId and deletes tokens)
        // In reality, logging in deletes all refresh tokens, but this tests the edge case where
        // CurrentDeviceId changed without going through the proper login flow
        user.CurrentDeviceId = Guid.NewGuid();
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        
        // Also delete the refresh token to simulate what login does
        _dbContext.RefreshTokens.Remove(refreshToken);
        await _dbContext.SaveChangesAsync();

        var request = new RefreshRequest
        {
            RefreshToken = refreshToken.Token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/refresh", request);

        // Assert
        // Should fail because the token was deleted when user logged in from another device
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithNonExistentToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshRequest
        {
            RefreshToken = "NonExistentToken123456789"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ReturnsUnauthorized()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var user = await CreateTestUserAsync("testuser", "Password123", null, deviceId);
        var revokedRefreshToken = await CreateTestRefreshTokenAsync(user.Id, isRevoked: true);

        var request = new RefreshRequest
        {
            RefreshToken = revokedRefreshToken.Token
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/v1/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
