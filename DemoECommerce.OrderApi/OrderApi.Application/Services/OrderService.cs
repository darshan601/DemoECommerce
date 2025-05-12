using System.Net.Http.Json;
using OrderApi.Application.Conversions;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;
using Polly;
using Polly.Registry;

namespace OrderApi.Application.Services;

public class OrderService(IOrder orderInterface,HttpClient httpClient, 
    ResiliencePipelineProvider<string> resiliencePipeline):IOrderService
{
    // GET Product
    public async Task<ProductDTO> GetProduct(int productId)
    {
        // call product api using Httpclient
        // redirect this call to the Api Gateway since product Api will not respond to outsiders.
        var getProduct = await httpClient.GetAsync($"/api/products/{productId}");
        
        if (!getProduct.IsSuccessStatusCode)
            return null!;

        var product = await getProduct.Content.ReadFromJsonAsync<ProductDTO>();
        return product!;

    }
    
    // Get user
    public async Task<AppUserDTO> GetUser(int userId)
    {
        // call product api using Httpclient
        // redirect this call to the Api Gateway since product Api will not respond to outsiders.
        var getUser = await httpClient.GetAsync($"/api/authentication/{userId}");
        
        if (!getUser.IsSuccessStatusCode)
            return null!;

        var user = await getUser.Content.ReadFromJsonAsync<AppUserDTO>();
        return user!;
    }
    
    // Get orders by client id
    public async Task<IEnumerable<OrderDTO>> GetOrdersByClientId(int clientId)
    {
        // Get all client's orders
        var orders = await orderInterface.GetOrdersAsync(o=> o.ClientId == clientId);
        
        if(!orders.Any()) return null!;
            
        // convert from entity to dto
        var (_, _orders) = OrderConversion.FromEntity(null, orders);

        return _orders!;
    }

    // get order details by id
    public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
    {
        // Prepare Order
        var order = await orderInterface.FindByIdAsync(orderId);
        if (order is null || order!.Id <= 0)
            return null!;
        
        // get retry pipeline
        var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");
        
        // prepare product
        var productDto= await retryPipeline.ExecuteAsync(async token => await GetProduct(order.ProductId));
        if (productDto is null)
            throw new Exception($"Product with ID {order.ProductId} not found.");
        
        // prepare Client
        var appUserDto = await retryPipeline.ExecuteAsync(async token => await GetUser(order.ClientId));
        if (appUserDto is null)
        throw new Exception($"User with ID {order.ClientId} not found.");
        
        // populate order details
        return new OrderDetailsDTO(
            order.Id,
            productDto.Id,
            appUserDto.Id,
            appUserDto.Name,
            appUserDto.Email,
            appUserDto.Address,
            appUserDto.TelephoneNumber,
            productDto.Name,
            order.PurchaseQuantity,
            productDto.Price,
            order.PurchaseQuantity * productDto.Price,
            order.OrderedDate
        );
    }
}