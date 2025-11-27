using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderApi.Models;
using OrderApi.Models.Dto;
using Xunit;

namespace OrderApi.IntegrationTests;

public class OrderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreated()
    {
        // Arrange
        var orderDto = new OrderForCreationDto
        {
            BookId = 1,
            BookTitle = "Integration Test Book",
            BookAuthor = "Test Author",
            Quantity = 5,
            UnitPrice = 29.99m,
            CustomerEmail = "test@integration.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdOrder = await response.Content.ReadFromJsonAsync<Order>();
        createdOrder.Should().NotBeNull();
        createdOrder!.BookId.Should().Be(orderDto.BookId);
        createdOrder.Quantity.Should().Be(orderDto.Quantity);
        createdOrder.TotalPrice.Should().Be(orderDto.Quantity * orderDto.UnitPrice);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidOrder = new OrderForCreationDto
        {
            BookId = 0, // Invalid
            BookTitle = "",
            BookAuthor = "",
            Quantity = -1, // Invalid
            UnitPrice = -10, // Invalid
            CustomerEmail = "invalid-email" // Invalid
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", invalidOrder);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderById_WithExistingId_ReturnsOrder()
    {
        // Arrange - Create an order first
        var orderDto = new OrderForCreationDto
        {
            BookId = 1,
            BookTitle = "Test Book",
            BookAuthor = "Test Author",
            Quantity = 3,
            UnitPrice = 19.99m,
            CustomerEmail = "test@example.com"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/orders", orderDto);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<Order>();

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var retrievedOrder = await response.Content.ReadFromJsonAsync<Order>();
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Id.Should().Be(createdOrder.Id);
        retrievedOrder.BookId.Should().Be(orderDto.BookId);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateMultipleOrders_AllSucceed()
    {
        // Arrange
        var orders = new[]
        {
            new OrderForCreationDto
            {
                BookId = 1,
                BookTitle = "Book 1",
                BookAuthor = "Author 1",
                Quantity = 2,
                UnitPrice = 15.99m,
                CustomerEmail = "customer1@test.com"
            },
            new OrderForCreationDto
            {
                BookId = 2,
                BookTitle = "Book 2",
                BookAuthor = "Author 2",
                Quantity = 3,
                UnitPrice = 25.99m,
                CustomerEmail = "customer2@test.com"
            },
            new OrderForCreationDto
            {
                BookId = 3,
                BookTitle = "Book 3",
                BookAuthor = "Author 3",
                Quantity = 1,
                UnitPrice = 35.99m,
                CustomerEmail = "customer3@test.com"
            }
        };

        // Act
        var tasks = orders.Select(order => _client.PostAsJsonAsync("/api/orders", order));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));
    }
}