using System.Security.Claims;
using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.Enums;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Models;
using BackSharedGroceries.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace BackSharedGroceries.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for ProductService.
/// Tests critical business logic like Last-Write-Wins, cross-family validation, and sync operations.
/// </summary>
public class ProductServiceUnitTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IFamilyRepository> _familyRepositoryMock;
    private readonly Mock<IShoppingListRepository> _shoppingListRepositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _productService;

    public ProductServiceUnitTests()
    {
        // Set required JWT environment variables for testing
        Environment.SetEnvironmentVariable("JWT_KEY", "test-secret-key-with-at-least-32-characters-for-hmac-sha256");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");

        _productRepositoryMock = new Mock<IProductRepository>();
        _familyRepositoryMock = new Mock<IFamilyRepository>();
        _shoppingListRepositoryMock = new Mock<IShoppingListRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        _productService = new ProductService(
            _productRepositoryMock.Object,
            _familyRepositoryMock.Object,
            _shoppingListRepositoryMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object
        );
    }

    private void SetupHttpContext(Guid userId, Guid? familyId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (familyId.HasValue)
        {
            claims.Add(new Claim("FamilyId", familyId.ToString()!));
        }

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

    #region GetProductSuggestionsAsync Tests

    [Fact]
    public async Task GetProductSuggestionsAsync_ReturnsOnlyUserFamilyProducts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        var expectedSuggestions = new List<string> { "Milk", "Bread", "Eggs" };
        _productRepositoryMock
            .Setup(x => x.GetProductSuggestionsAsync(familyId))
            .ReturnsAsync(expectedSuggestions);

        // Act
        var result = await _productService.GetProductSuggestionsAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedSuggestions);
        
        _familyRepositoryMock.Verify(x => x.GetUserFamilyIdAsync(userId), Times.Once);
        _productRepositoryMock.Verify(x => x.GetProductSuggestionsAsync(familyId), Times.Once);
    }

    [Fact]
    public async Task GetProductSuggestionsAsync_WhenUserHasNoFamily_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupHttpContext(userId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _productService.GetProductSuggestionsAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.BadRequest);
        result.ErrorMessage.Should().Contain("does not belong to a family");
    }

    #endregion

    #region SyncBatchAsync Cross-Family Protection Tests

    [Fact]
    public async Task SyncBatchAsync_IgnoresProductsFromOtherFamilyLists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userFamilyId = Guid.NewGuid();
        var validListId = Guid.NewGuid();
        var invalidListId = Guid.NewGuid(); // Not in user's family
        
        SetupHttpContext(userId, userFamilyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(userFamilyId);

        // Only validListId belongs to the user's family
        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(userFamilyId))
            .ReturnsAsync(new List<Guid> { validListId });

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(true);

        var batch = new SyncBatchDto
        {
            Products = new List<ProductUpsertDto>
            {
                new ProductUpsertDto
                {
                    Id = Guid.NewGuid(),
                    ListId = validListId, // Valid
                    Name = "Valid Product",
                    Quantity = "1",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = DateTime.UtcNow
                },
                new ProductUpsertDto
                {
                    Id = Guid.NewGuid(),
                    ListId = invalidListId, // Invalid - different family
                    Name = "Invalid Product",
                    Quantity = "1",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = DateTime.UtcNow
                }
            }
        };

        // Act
        var result = await _productService.SyncBatchAsync(batch);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalProcessed.Should().Be(2);
        result.Data.Synced.Should().HaveCount(1); // Only valid product
        result.Data.Ignored.Should().HaveCount(1); // Invalid product ignored

        // Verify upsert was only called once (for the valid product)
        _productRepositoryMock.Verify(
            x => x.UpsertProductAsync(It.Is<Product>(p => p.ListId == validListId)), 
            Times.Once
        );
    }

    [Fact]
    public async Task SyncBatchAsync_PartialSuccessScenario()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var validListId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(familyId))
            .ReturnsAsync(new List<Guid> { validListId });

        // First product succeeds, second fails (e.g., database constraint)
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.Is<Product>(p => p.Id == product1Id)))
            .ReturnsAsync(true);

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.Is<Product>(p => p.Id == product2Id)))
            .ReturnsAsync(false); // Failure (e.g., timestamp conflict)

        var batch = new SyncBatchDto
        {
            Products = new List<ProductUpsertDto>
            {
                new ProductUpsertDto
                {
                    Id = product1Id,
                    ListId = validListId,
                    Name = "Product 1",
                    Quantity = "1",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = DateTime.UtcNow
                },
                new ProductUpsertDto
                {
                    Id = product2Id,
                    ListId = validListId,
                    Name = "Product 2",
                    Quantity = "1",
                    Status = ProductStatus.Pending,
                    ClientTimestamp = DateTime.UtcNow.AddMinutes(-10) // Older timestamp
                }
            }
        };

        // Act
        var result = await _productService.SyncBatchAsync(batch);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Synced.Should().Contain(product1Id);
        result.Data.Ignored.Should().Contain(product2Id);
    }

    #endregion

    #region AddProductAsync Tests

    [Fact]
    public async Task AddProductAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(familyId))
            .ReturnsAsync(new List<Guid> { listId });

        var createdProduct = new Product
        {
            Id = Guid.NewGuid(),
            ListId = listId,
            Name = "New Product",
            Quantity = "2",
            Status = ProductStatus.Pending,
            LastModifiedByUserId = userId
        };

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(true);

        var dto = new ProductUpsertDto
        {
            Id = createdProduct.Id,
            ListId = listId,
            Name = "New Product",
            Quantity = "2",
            Status = ProductStatus.Pending,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var result = await _productService.AddProductAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Product");
        result.Data.Quantity.Should().Be("2");
    }

    [Fact]
    public async Task AddProductAsync_WithCrossFamilyListId_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userFamilyId = Guid.NewGuid();
        var otherFamilyListId = Guid.NewGuid();
        
        SetupHttpContext(userId, userFamilyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(userFamilyId);

        // User's family has no lists
        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(userFamilyId))
            .ReturnsAsync(new List<Guid>());

        var dto = new ProductUpsertDto
        {
            Id = Guid.NewGuid(),
            ListId = otherFamilyListId, // Belongs to different family
            Name = "Hacked Product",
            Quantity = "1",
            Status = ProductStatus.Pending,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var result = await _productService.AddProductAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
    }

    #endregion

    #region DeleteProductAsync Tests

    [Fact]
    public async Task DeleteProductAsync_WithValidProduct_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        var product = new Product
        {
            Id = productId,
            ListId = listId,
            Name = "Test Product",
            Quantity = "1",
            Status = ProductStatus.Pending,
            LastModifiedByUserId = userId
        };

        _productRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(familyId))
            .ReturnsAsync(new List<Guid> { listId });

        _productRepositoryMock
            .Setup(x => x.DeleteProductAsync(productId))
            .ReturnsAsync(true);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Success.Should().BeTrue();
        _productRepositoryMock.Verify(x => x.DeleteProductAsync(productId), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _productRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.NotFound);
    }

    [Fact]
    public async Task DeleteProductAsync_CrossFamilyProduct_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userFamilyId = Guid.NewGuid();
        var otherFamilyListId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        SetupHttpContext(userId, userFamilyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(userFamilyId);

        var product = new Product
        {
            Id = productId,
            ListId = otherFamilyListId, // Different family's list
            Name = "Other Family Product",
            Quantity = "1",
            Status = ProductStatus.Pending,
            LastModifiedByUserId = Guid.NewGuid()
        };

        _productRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        // User's family has no lists
        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(userFamilyId))
            .ReturnsAsync(new List<Guid>());

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Success.Should().BeFalse();
        result.ResultType.Should().Be(ServiceResultType.Unauthorized);
        _productRepositoryMock.Verify(x => x.DeleteProductAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Auto-Deactivation Tests

    [Fact]
    public async Task AddProductAsync_WhenAllProductsBecomePaid_AutoDeactivatesList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(familyId))
            .ReturnsAsync(new List<Guid> { listId });

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(true);

        // After adding this product, all products are paid
        _shoppingListRepositoryMock
            .Setup(x => x.AreAllProductsPaidAsync(listId))
            .ReturnsAsync(true);

        _shoppingListRepositoryMock
            .Setup(x => x.UpdateShoppingListStatusAsync(listId, false))
            .Returns(Task.CompletedTask);

        var dto = new ProductUpsertDto
        {
            Id = Guid.NewGuid(),
            Name = "Milk",
            Quantity = "1L",
            Status = ProductStatus.Paid,
            ListId = listId,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var result = await _productService.AddProductAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify that the list was auto-deactivated
        _shoppingListRepositoryMock.Verify(
            x => x.AreAllProductsPaidAsync(listId), 
            Times.Once
        );
        _shoppingListRepositoryMock.Verify(
            x => x.UpdateShoppingListStatusAsync(listId, false), 
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProductAsync_WhenAllProductsBecomePaid_AutoDeactivatesList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(familyId))
            .ReturnsAsync(new List<Guid> { listId });

        var existingProduct = new Product
        {
            Id = productId,
            ListId = listId,
            Name = "Milk",
            Quantity = "1L",
            Status = ProductStatus.InCart,
            LastModifiedByUserId = userId,
            ClientTimestamp = DateTime.UtcNow.AddMinutes(-5)
        };

        _productRepositoryMock
            .Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(true);

        // After updating this product to Paid, all products are now paid
        _shoppingListRepositoryMock
            .Setup(x => x.AreAllProductsPaidAsync(listId))
            .ReturnsAsync(true);

        _shoppingListRepositoryMock
            .Setup(x => x.UpdateShoppingListStatusAsync(listId, false))
            .Returns(Task.CompletedTask);

        var dto = new ProductUpsertDto
        {
            Id = productId,
            Name = "Milk",
            Quantity = "1L",
            Status = ProductStatus.Paid, // Changed to Paid
            ListId = listId,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var result = await _productService.UpdateProductAsync(productId, dto);

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify that the list was auto-deactivated
        _shoppingListRepositoryMock.Verify(
            x => x.AreAllProductsPaidAsync(listId), 
            Times.Once
        );
        _shoppingListRepositoryMock.Verify(
            x => x.UpdateShoppingListStatusAsync(listId, false), 
            Times.Once
        );
    }

    [Fact]
    public async Task AddProductAsync_WhenNotAllProductsPaid_DoesNotDeactivateList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(familyId))
            .ReturnsAsync(new List<Guid> { listId });

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(true);

        // Not all products are paid
        _shoppingListRepositoryMock
            .Setup(x => x.AreAllProductsPaidAsync(listId))
            .ReturnsAsync(false);

        var dto = new ProductUpsertDto
        {
            Id = Guid.NewGuid(),
            Name = "Milk",
            Quantity = "1L",
            Status = ProductStatus.Pending,
            ListId = listId,
            ClientTimestamp = DateTime.UtcNow
        };

        // Act
        var result = await _productService.AddProductAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify that the list was NOT deactivated
        _shoppingListRepositoryMock.Verify(
            x => x.UpdateShoppingListStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task SyncBatchAsync_WhenAllProductsBecomePaid_AutoDeactivatesList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        
        SetupHttpContext(userId, familyId);

        _familyRepositoryMock
            .Setup(x => x.GetUserFamilyIdAsync(userId))
            .ReturnsAsync(familyId);

        _productRepositoryMock
            .Setup(x => x.GetFamilyListIdsAsync(familyId))
            .ReturnsAsync(new List<Guid> { listId });

        _productRepositoryMock
            .Setup(x => x.UpsertProductAsync(It.IsAny<Product>()))
            .ReturnsAsync(true);

        // After syncing, all products are paid
        _shoppingListRepositoryMock
            .Setup(x => x.AreAllProductsPaidAsync(listId))
            .ReturnsAsync(true);

        _shoppingListRepositoryMock
            .Setup(x => x.UpdateShoppingListStatusAsync(listId, false))
            .Returns(Task.CompletedTask);

        var batch = new SyncBatchDto
        {
            Products = new List<ProductUpsertDto>
            {
                new ProductUpsertDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Milk",
                    Quantity = "1L",
                    Status = ProductStatus.Paid,
                    ListId = listId,
                    ClientTimestamp = DateTime.UtcNow
                },
                new ProductUpsertDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Bread",
                    Quantity = "1",
                    Status = ProductStatus.Paid,
                    ListId = listId,
                    ClientTimestamp = DateTime.UtcNow
                }
            }
        };

        // Act
        var result = await _productService.SyncBatchAsync(batch);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Synced.Should().HaveCount(2);
        
        // Verify that the list was checked and auto-deactivated for each product
        _shoppingListRepositoryMock.Verify(
            x => x.AreAllProductsPaidAsync(listId), 
            Times.Exactly(2) // Once per product
        );
        _shoppingListRepositoryMock.Verify(
            x => x.UpdateShoppingListStatusAsync(listId, false), 
            Times.Exactly(2) // Called each time all products are paid
        );
    }

    #endregion
}
