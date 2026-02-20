using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Models;
using BackSharedGroceries.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BackSharedGroceries.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for AuthService.
/// Tests business logic in isolation using mocked repositories.
/// </summary>
public class AuthServiceUnitTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceUnitTests()
    {
        // Set required JWT environment variables for testing
        Environment.SetEnvironmentVariable("JWT_KEY", "test-secret-key-with-at-least-32-characters-for-hmac-sha256");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");

        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        
        _authService = new AuthService(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithNewUsername_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "SecurePassword123!"
        };

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(request.Username))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.ResultType.Should().Be(ServiceResultType.Ok);

        _userRepositoryMock.Verify(x => x.ExistsAsync(request.Username), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => 
            u.Username == request.Username && 
            !string.IsNullOrEmpty(u.PasswordHash)
        )), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Password = "SecurePassword123!"
        };

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(request.Username))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Conflict);
        result.ErrorMessage.Should().Contain("already exists");

        _userRepositoryMock.Verify(x => x.ExistsAsync(request.Username), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var password = "MyPassword123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FamilyId = Guid.NewGuid()
        };

        var request = new LoginRequest
        {
            Username = user.Username,
            Password = password
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        _refreshTokenRepositoryMock
            .Setup(x => x.DeleteAllForUserAsync(user.Id))
            .Returns(Task.CompletedTask);

        _refreshTokenRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Token = "test-refresh-token" });

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.ResultType.Should().Be(ServiceResultType.Ok);
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.Username.Should().Be(user.Username);
        result.Data.FamilyId.Should().Be(user.FamilyId);

        _refreshTokenRepositoryMock.Verify(x => x.DeleteAllForUserAsync(user.Id), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.CurrentDeviceId != null)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
        };

        var request = new LoginRequest
        {
            Username = user.Username,
            Password = "WrongPassword"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
        result.ErrorMessage.Should().Contain("Invalid credentials");

        _refreshTokenRepositoryMock.Verify(x => x.DeleteAllForUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "SomePassword"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewJwt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hashedpassword",
            FamilyId = Guid.NewGuid(),
            CurrentDeviceId = deviceId
        };

        var refreshToken = new RefreshToken
        {
            Token = "validrefreshtoken",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            User = user
        };

        var request = new RefreshRequest
        {
            RefreshToken = refreshToken.Token
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenWithUserAsync(request.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.ResultType.Should().Be(ServiceResultType.Ok);
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "expiredtoken",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            IsRevoked = false,
            User = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hash",
                CurrentDeviceId = Guid.NewGuid()
            }
        };

        var request = new RefreshRequest
        {
            RefreshToken = refreshToken.Token
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenWithUserAsync(request.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
        result.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "revokedtoken",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = true, // Revoked
            User = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hash",
                CurrentDeviceId = Guid.NewGuid()
            }
        };

        var request = new RefreshRequest
        {
            RefreshToken = refreshToken.Token
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenWithUserAsync(request.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNullCurrentDeviceId_ReturnsUnauthorized()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "validtoken",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false,
            User = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hash",
                CurrentDeviceId = null // No active device session
            }
        };

        var request = new RefreshRequest
        {
            RefreshToken = refreshToken.Token
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenWithUserAsync(request.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
    }

    #endregion
}
