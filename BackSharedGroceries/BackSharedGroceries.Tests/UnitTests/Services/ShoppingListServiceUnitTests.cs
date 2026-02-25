using System.Security.Claims;
using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.Hubs;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Models;
using BackSharedGroceries.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace BackSharedGroceries.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for ShoppingListService.
/// Tests critical business logic like family validation, soft delete, restore, and security checks.
/// </summary>
public class ShoppingListServiceUnitTests
{
    private readonly Mock<IShoppingListRepository> _shoppingListRepositoryMock;
    private readonly Mock<IFamilyRepository> _familyRepositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IHubContext<ShoppingListHub>> _hubContextMock;
    private readonly ShoppingListService _shoppingListService;

    public ShoppingListServiceUnitTests()
    {
        // Set required JWT environment variables for testing
        Environment.SetEnvironmentVariable("JWT_KEY", "test-secret-key-with-at-least-32-characters-for-hmac-sha256");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");

        _shoppingListRepositoryMock = new Mock<IShoppingListRepository>();
        _familyRepositoryMock = new Mock<IFamilyRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // Hub context mock — hub calls aren't verified in unit tests, just need it to not throw.
        _hubContextMock = new Mock<IHubContext<ShoppingListHub>>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        clientProxyMock
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
        _hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);

        _shoppingListService = new ShoppingListService(
            _shoppingListRepositoryMock.Object,
            _familyRepositoryMock.Object,
            _httpContextAccessorMock.Object,
            _hubContextMock.Object
        );
    }

    private void SetupHttpContext(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);
    }

    #region CreateShoppingListAsync Tests

    [Fact]
    public async Task CreateShoppingListAsync_WithValidData_ReturnsCreatedList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _shoppingListRepositoryMock
            .Setup(x => x.CreateShoppingListAsync(It.IsAny<ShoppingList>()))
            .Returns(Task.CompletedTask);

        var request = new CreateShoppingListRequest
        {
            Name = "Weekly Groceries"
        };

        // Act
        var result = await _shoppingListService.CreateShoppingListAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Weekly Groceries");
        result.Data.FamilyId.Should().Be(familyId);
        result.Data.IsActive.Should().BeTrue();

        _shoppingListRepositoryMock.Verify(
            x => x.CreateShoppingListAsync(It.Is<ShoppingList>(sl => 
                sl.Name == "Weekly Groceries" && 
                sl.FamilyId == familyId &&
                sl.IsActive == true
            )), 
            Times.Once
        );
    }

    [Fact]
    public async Task CreateShoppingListAsync_WhenUserHasNoFamily_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync((Guid?)null);

        var request = new CreateShoppingListRequest
        {
            Name = "Weekly Groceries"
        };

        // Act
        var result = await _shoppingListService.CreateShoppingListAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.BadRequest);
        result.ErrorMessage.Should().Contain("must belong to a family");

        _shoppingListRepositoryMock.Verify(
            x => x.CreateShoppingListAsync(It.IsAny<ShoppingList>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task CreateShoppingListAsync_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns((HttpContext?)null);

        var request = new CreateShoppingListRequest
        {
            Name = "Weekly Groceries"
        };

        // Act
        var result = await _shoppingListService.CreateShoppingListAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.BadRequest);
    }

    #endregion

    #region GetFamilyShoppingListsAsync Tests

    [Fact]
    public async Task GetFamilyShoppingListsAsync_ReturnsOnlyActiveListsByDefault()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        var allLists = new List<ShoppingList>
        {
            new ShoppingList { Id = Guid.NewGuid(), Name = "Active List", FamilyId = familyId, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ShoppingList { Id = Guid.NewGuid(), Name = "Inactive List", FamilyId = familyId, IsActive = false, CreatedAt = DateTime.UtcNow }
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListsByFamilyIdAsync(familyId))
            .ReturnsAsync(allLists);

        // Act
        var result = await _shoppingListService.GetFamilyShoppingListsAsync(includeInactive: false);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Data!.First().Name.Should().Be("Active List");
        result.Data!.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetFamilyShoppingListsAsync_WithIncludeInactive_ReturnsAllLists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        var allLists = new List<ShoppingList>
        {
            new ShoppingList { Id = Guid.NewGuid(), Name = "Active List", FamilyId = familyId, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ShoppingList { Id = Guid.NewGuid(), Name = "Inactive List", FamilyId = familyId, IsActive = false, CreatedAt = DateTime.UtcNow }
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListsByFamilyIdAsync(familyId))
            .ReturnsAsync(allLists);

        // Act
        var result = await _shoppingListService.GetFamilyShoppingListsAsync(includeInactive: true);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFamilyShoppingListsAsync_WhenUserHasNoFamily_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _shoppingListService.GetFamilyShoppingListsAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.BadRequest);
        result.ErrorMessage.Should().Contain("must belong to a family");
    }

    #endregion

    #region SoftDeleteShoppingListAsync Tests

    [Fact]
    public async Task SoftDeleteShoppingListAsync_WithValidList_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        var shoppingList = new ShoppingList
        {
            Id = listId,
            Name = "Test List",
            FamilyId = familyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListByIdAsync(listId))
            .ReturnsAsync(shoppingList);

        _shoppingListRepositoryMock
            .Setup(x => x.SoftDeleteShoppingListAsync(listId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingListService.SoftDeleteShoppingListAsync(listId);

        // Assert
        result.Success.Should().BeTrue();
        _shoppingListRepositoryMock.Verify(x => x.SoftDeleteShoppingListAsync(listId), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteShoppingListAsync_WithNonexistentList_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListByIdAsync(listId))
            .ReturnsAsync((ShoppingList?)null);

        // Act
        var result = await _shoppingListService.SoftDeleteShoppingListAsync(listId);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.NotFound);
        result.ErrorMessage.Should().Contain("not found");

        _shoppingListRepositoryMock.Verify(x => x.SoftDeleteShoppingListAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SoftDeleteShoppingListAsync_WithOtherFamilyList_ReturnsUnauthorized()
    {
        // Arrange - Security test: User tries to delete another family's list
        var userId = Guid.NewGuid();
        var userFamilyId = Guid.NewGuid();
        var otherFamilyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(userFamilyId);

        var shoppingList = new ShoppingList
        {
            Id = listId,
            Name = "Other Family List",
            FamilyId = otherFamilyId, // Different family!
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListByIdAsync(listId))
            .ReturnsAsync(shoppingList);

        // Act
        var result = await _shoppingListService.SoftDeleteShoppingListAsync(listId);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
        result.ErrorMessage.Should().Contain("permission");

        _shoppingListRepositoryMock.Verify(x => x.SoftDeleteShoppingListAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region RestoreShoppingListAsync Tests

    [Fact]
    public async Task RestoreShoppingListAsync_WithValidList_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        var shoppingList = new ShoppingList
        {
            Id = listId,
            Name = "Deleted List",
            FamilyId = familyId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListByIdAsync(listId))
            .ReturnsAsync(shoppingList);

        _shoppingListRepositoryMock
            .Setup(x => x.RestoreShoppingListAsync(listId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingListService.RestoreShoppingListAsync(listId);

        // Assert
        result.Success.Should().BeTrue();
        _shoppingListRepositoryMock.Verify(x => x.RestoreShoppingListAsync(listId), Times.Once);
    }

    [Fact]
    public async Task RestoreShoppingListAsync_WithOtherFamilyList_ReturnsUnauthorized()
    {
        // Arrange - Security test: User tries to restore another family's list
        var userId = Guid.NewGuid();
        var userFamilyId = Guid.NewGuid();
        var otherFamilyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(userFamilyId);

        var shoppingList = new ShoppingList
        {
            Id = listId,
            Name = "Other Family List",
            FamilyId = otherFamilyId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListByIdAsync(listId))
            .ReturnsAsync(shoppingList);

        // Act
        var result = await _shoppingListService.RestoreShoppingListAsync(listId);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
        result.ErrorMessage.Should().Contain("permission");

        _shoppingListRepositoryMock.Verify(x => x.RestoreShoppingListAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region UpdateShoppingListStatusAsync Tests

    [Fact]
    public async Task UpdateShoppingListStatusAsync_WithValidList_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        var shoppingList = new ShoppingList
        {
            Id = listId,
            Name = "Test List",
            FamilyId = familyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListByIdAsync(listId))
            .ReturnsAsync(shoppingList);

        _shoppingListRepositoryMock
            .Setup(x => x.UpdateShoppingListStatusAsync(listId, false))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingListService.UpdateShoppingListStatusAsync(listId, false);

        // Assert
        result.Success.Should().BeTrue();
        _shoppingListRepositoryMock.Verify(x => x.UpdateShoppingListStatusAsync(listId, false), Times.Once);
    }

    [Fact]
    public async Task UpdateShoppingListStatusAsync_WithOtherFamilyList_ReturnsUnauthorized()
    {
        // Arrange - Security test
        var userId = Guid.NewGuid();
        var userFamilyId = Guid.NewGuid();
        var otherFamilyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(userFamilyId);

        var shoppingList = new ShoppingList
        {
            Id = listId,
            Name = "Other Family List",
            FamilyId = otherFamilyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _shoppingListRepositoryMock
            .Setup(x => x.GetShoppingListByIdAsync(listId))
            .ReturnsAsync(shoppingList);

        // Act
        var result = await _shoppingListService.UpdateShoppingListStatusAsync(listId, false);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
        result.ErrorMessage.Should().Contain("permission");

        _shoppingListRepositoryMock.Verify(x => x.UpdateShoppingListStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region CheckAndAutoDeactivateListAsync Tests

    [Fact]
    public async Task CheckAndAutoDeactivateListAsync_WhenAllProductsPaid_ReturnsTrueAndDeactivates()
    {
        // Arrange
        var listId = Guid.NewGuid();

        _shoppingListRepositoryMock
            .Setup(x => x.AreAllProductsPaidAsync(listId))
            .ReturnsAsync(true);

        _shoppingListRepositoryMock
            .Setup(x => x.UpdateShoppingListStatusAsync(listId, false))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _shoppingListService.CheckAndAutoDeactivateListAsync(listId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        _shoppingListRepositoryMock.Verify(x => x.UpdateShoppingListStatusAsync(listId, false), Times.Once);
    }

    [Fact]
    public async Task CheckAndAutoDeactivateListAsync_WhenNotAllProductsPaid_ReturnsFalse()
    {
        // Arrange
        var listId = Guid.NewGuid();

        _shoppingListRepositoryMock
            .Setup(x => x.AreAllProductsPaidAsync(listId))
            .ReturnsAsync(false);

        // Act
        var result = await _shoppingListService.CheckAndAutoDeactivateListAsync(listId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeFalse();
        _shoppingListRepositoryMock.Verify(x => x.UpdateShoppingListStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion
}
