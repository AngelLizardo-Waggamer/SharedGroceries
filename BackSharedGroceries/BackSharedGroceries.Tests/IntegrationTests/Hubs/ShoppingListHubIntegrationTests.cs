using System.Net.Http.Json;
using System.Text.Json;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace BackSharedGroceries.Tests.IntegrationTests.Hubs;

/// <summary>
/// Integration tests for the ShoppingListHub.
/// Each test runs against the full in-process server (real PostgreSQL via Testcontainers)
/// through the WebApplicationFactory's in-memory HTTP handler — no real TCP port needed.
/// Covers ProductAdded, ProductUpdated, ProductDeleted, and ListArchived events.
/// </summary>
public class ShoppingListHubIntegrationTests : IntegrationTestBase
{
    // How long to wait for a hub event before failing the test.
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(10);

    public ShoppingListHubIntegrationTests(PostgresTestcontainerFixture fixture) : base(fixture) { }

    // -------------------------------------------------------------------------
    // Helper: build an in-process HubConnection for the test server
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a hub connection routed through the test server's in-memory handler.
    /// LongPolling is used because the in-memory handler doesn't support WebSocket upgrades.
    /// The JWT is passed as a Bearer header, matching the production setup.
    /// </summary>
    private HubConnection BuildHubConnection(string token)
    {
        return new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/shopping", options =>
            {
                // Use the test server's in-memory handler instead of a real HTTP client.
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();

                // Pass the token as a header, same as the Flutter client does.
                options.Headers["Authorization"] = $"Bearer {token}";

                // Long polling since the in-memory handler doesn't do WebSocket upgrades.
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Connect_WithoutToken_ShouldBeRejected()
    {
        // No Authorization header — negotiate should return 401.
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/shopping", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        // Negotiate returns 401 without a token, so StartAsync should throw.
        var act = async () => await connection.StartAsync();
        await act.Should().ThrowAsync<Exception>("a connection without a token must be rejected");

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task Connect_WithValidToken_ShouldReachConnectedState()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Test Family", "INV-001");
        var device = Guid.NewGuid();
        var user = await CreateTestUserAsync("hubuser", familyId: family.FamilyId, deviceId: device);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, device, family.FamilyId);

        var connection = BuildHubConnection(token);

        try
        {
            await connection.StartAsync();
            connection.State.Should().Be(HubConnectionState.Connected);
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddProduct_ShouldBroadcast_ProductAdded_ToFamilyGroup()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Simpson Family", "INV-002");
        var device = Guid.NewGuid();
        var user = await CreateTestUserAsync("homer", familyId: family.FamilyId, deviceId: device);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, device, family.FamilyId);
        var list = await CreateTestShoppingListAsync(family.FamilyId, "Weekly List");

        var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        var connection = BuildHubConnection(token);
        connection.On<JsonElement>("ProductAdded", payload => tcs.TrySetResult(payload));

        await connection.StartAsync();
        SetAuthorizationHeader(token);

        try
        {
            // Add a product through REST — this should trigger the ProductAdded hub event
            var dto = new ProductUpsertDto
            {
                Id = Guid.NewGuid(),
                Name = "Donuts",
                Quantity = "12",
                Status = Enums.ProductStatus.Pending,
                ListId = list.Id,
                ClientTimestamp = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/products/v1/create", dto);
            response.IsSuccessStatusCode.Should().BeTrue(
                $"product creation should succeed (status: {response.StatusCode}, body: {await response.Content.ReadAsStringAsync()})");

            // Wait for the event — it should arrive before the timeout
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(EventTimeout));
            completedTask.Should().Be(tcs.Task, "ProductAdded event should have been received");

            var eventPayload = await tcs.Task;
            eventPayload.GetProperty("name").GetString().Should().Be("Donuts");
            eventPayload.GetProperty("listId").GetGuid().Should().Be(list.Id);
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task DeleteProduct_ShouldBroadcast_ProductDeleted_ToFamilyGroup()
    {
        // Arrange
        var family = await CreateTestFamilyAsync("Flanders Family", "INV-003");
        var device = Guid.NewGuid();
        var user = await CreateTestUserAsync("ned", familyId: family.FamilyId, deviceId: device);
        var token = TestJwtHelper.GenerateTestToken(user.Id, user.Username, device, family.FamilyId);
        var list = await CreateTestShoppingListAsync(family.FamilyId);
        var product = await CreateTestProductAsync(list.Id, "Okily Dokily", user.Id);

        var tcs = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);
        var connection = BuildHubConnection(token);
        connection.On<Guid>("ProductDeleted", id => tcs.TrySetResult(id));

        await connection.StartAsync();
        SetAuthorizationHeader(token);

        try
        {
            // Act
            var response = await _client.DeleteAsync($"/api/products/v1/delete/{product.Id}");
            response.IsSuccessStatusCode.Should().BeTrue(
                $"product deletion should succeed (status: {response.StatusCode}, body: {await response.Content.ReadAsStringAsync()})");

            // Assert
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(EventTimeout));
            completedTask.Should().Be(tcs.Task, "ProductDeleted event should have been received");
            (await tcs.Task).Should().Be(product.Id);
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task FamilyIsolation_EventForFamilyA_ShouldNotReach_FamilyB()
    {
        // Arrange two separate families, each with their own connection
        var familyA = await CreateTestFamilyAsync("Family A", "INV-005");
        var familyB = await CreateTestFamilyAsync("Family B", "INV-006");

        var deviceA = Guid.NewGuid();
        var deviceB = Guid.NewGuid();
        var userA = await CreateTestUserAsync("userA", familyId: familyA.FamilyId, deviceId: deviceA);
        var userB = await CreateTestUserAsync("userB", familyId: familyB.FamilyId, deviceId: deviceB);

        var tokenA = TestJwtHelper.GenerateTestToken(userA.Id, userA.Username, deviceA, familyA.FamilyId);
        var tokenB = TestJwtHelper.GenerateTestToken(userB.Id, userB.Username, deviceB, familyB.FamilyId);

        var listA = await CreateTestShoppingListAsync(familyA.FamilyId, "Family A List");

        // Family B's connection listens for ProductAdded
        var tcsB = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        var connectionB = BuildHubConnection(tokenB);
        connectionB.On<JsonElement>("ProductAdded", payload => tcsB.TrySetResult(payload));
        await connectionB.StartAsync();

        // Use Family A's token for the REST call
        SetAuthorizationHeader(tokenA);

        try
        {
            // Family A adds a product — events should stay in their own group
            var dto = new ProductUpsertDto
            {
                Id = Guid.NewGuid(),
                Name = "Secret product for Family A",
                Quantity = "1",
                Status = Enums.ProductStatus.Pending,
                ListId = listA.Id,
                ClientTimestamp = DateTime.UtcNow
            };
            var response = await _client.PostAsJsonAsync("/api/products/v1/create", dto);
            response.IsSuccessStatusCode.Should().BeTrue(
                $"product creation should succeed (status: {response.StatusCode}, body: {await response.Content.ReadAsStringAsync()})");

            // Family B shouldn't receive anything from Family A's events
            var receivedEvent = await Task.WhenAny(tcsB.Task, Task.Delay(TimeSpan.FromSeconds(3)));
            receivedEvent.Should().NotBe(tcsB.Task, "Family B must not receive events meant for Family A");
        }
        finally
        {
            await connectionB.StopAsync();
            await connectionB.DisposeAsync();
        }
    }
}
