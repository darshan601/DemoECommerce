using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Services;
using OrderApi.Domain.Entities;

namespace UnitTest.OrderApi.Services;

public class OrderServiceTest
{
    private readonly IOrderService orderServiceInterface;
    private readonly IOrder orderInterface;

    public OrderServiceTest()
    {
        orderServiceInterface = A.Fake<IOrderService>();
        orderInterface = A.Fake<IOrder>();
    }

    // Create fake httpmessageHandler
    public class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response = response;


        protected override Task<HttpResponseMessage> 
            SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
    
    // CREATE FAKE HTTP CLIENT USING FAKE HTTP MESSAGE HANDLER
    private static HttpClient CreateFakeHttpClient(object o)
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(o)
        };
        var fakeHttpMessageHandler = new FakeHttpMessageHandler(httpResponseMessage);
        var _httpClient = new HttpClient(fakeHttpMessageHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };
        return _httpClient;
    }
    
    
    // GET PRODUCT
    [Fact]
    public async Task GetProduct_ValidProductId_ReturnProduct()
    {
        // Arrange
        int productId = 1;
        var productDto = new ProductDTO(1, "Product 1", 13, 15.55m);
        var _httpClient = CreateFakeHttpClient(productDto);

        // System under test
        // we only need httpclient to make calls  
        // specify only httpclient and Null to the rest
        var _orderService = new OrderService(null!, _httpClient, null!);

        // Act
        var result = await _orderService.GetProduct(productId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Product 1");

    }

    [Fact]
    public async Task GetProduct_InvalidProductId_ReturnNull()
    {
        int productId = 1;
        var _httpClient = CreateFakeHttpClient(null!);
        var _orderService = new OrderService(null!, _httpClient, null!);
        
        // Act
        var result = await _orderService.GetProduct(productId);
        
        // Assert
        result.Should().BeNull();
    }
    
    // GET CLIENT ORDERS BY ID
    [Fact]
    public async Task GetOrdersByClientIs_OrderExists_ReturnOrderDetails()
    {
        // Arange
        int clientId = 1;
        var orders = new List<Order>
        {
            new() { Id = 1, ProductId = 1, ClientId = clientId, PurchaseQuantity = 2, OrderedDate = DateTime.UtcNow },
            new()
            {
                Id = 1, ProductId = 2, ClientId = clientId, PurchaseQuantity = 1,
                OrderedDate = DateTime.UtcNow
            }
        };
        
        // mock the getOrdersBy method
        A.CallTo(() => orderInterface.GetOrdersAsync
            (A<Expression<Func<Order, bool>>>.Ignored)).Returns(orders);
        var _orderService = new OrderService(orderInterface, null!, null!);
        
        // act
        var result = await _orderService.GetOrdersByClientId(clientId);

        // assert
        result.Should().NotBeNull();
        result.Should().HaveCount(orders.Count);
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }
    
    
}