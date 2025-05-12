using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.Conversions;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Services;

namespace OrderApi.Presentation.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize]
public class OrdersController(IOrder orderInterface, IOrderService orderService):ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
    {
        var orders = await orderInterface.GetAllAsync();
        if (!orders.Any())
            return NotFound("No orders found");

        var (_, list) = OrderConversion.FromEntity(null!, orders);
        
        return !list!.Any()
            ? NotFound() 
            : Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDTO>> GetOrder(int id)
    {
        var order = await orderInterface.FindByIdAsync(id);
        if (order is null)
            return NotFound(null);

        var (_order, _) = OrderConversion.FromEntity(order, null);

        return Ok(order);
    }

    [HttpGet("client/{clientId:int}")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetClientOrders(int clientId)
    {
        if (clientId <= 0)
            return BadRequest("Invalid Data Provided");

        var list = await orderService.GetOrdersByClientId(clientId);

        // if (!orders.Any()) return NotFound("No Orders Found");
        //
        // var (_, list) = OrderConversion.FromEntity(null, orders);
        
        return !list!.Any()
            ? NotFound() 
            : Ok(list);
    }

    [HttpGet("details/{orderId:int}")]
    public async Task<ActionResult<OrderDetailsDTO>> GetOrderDetails(int orderId)
    {
        if (orderId <= 0)
            return BadRequest("Invalid Data Provided");

        var orderDetail = await orderService.GetOrderDetails(orderId);
        return orderDetail.OrderId > 0 ? Ok(orderDetail) : NotFound("No Order Found");
    }

    [HttpPost]
    public async Task<ActionResult<Response>> CreateOrder(OrderDTO orderDto)
    {
        // check model state if all data annotations are passed
        if (!ModelState.IsValid)
            return BadRequest("Incomplete data submitted");
        
        // convert to entity
        var getEntity = OrderConversion.ToEntity(orderDto);

        var response = await orderInterface.CreateAsync(getEntity);

        return response.Flag? Ok(response) : BadRequest(response);

    }

    [HttpPut]
    public async Task<ActionResult<Response>> UpdateOrder(OrderDTO orderDto)
    {
        // check model state if all data annotations are passed
        if (!ModelState.IsValid)
            return BadRequest("Incomplete data submitted");
        
        // convert from dto to entity
        var order = OrderConversion.ToEntity(orderDto);

        var response = await orderInterface.UpdateAsync(order);

        return response.Flag
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpDelete]
    public async Task<ActionResult<Response>> DeleteOrder(OrderDTO orderDto)
    {
        // check model state if all data annotations are passed
        if (!ModelState.IsValid)
            return BadRequest("Incomplete data submitted");

        // convert from dto to entity
        var order = OrderConversion.ToEntity(orderDto);

        var response = await orderInterface.DeleteAsync(order);

        return response.Flag
            ? Ok(response)
            : BadRequest(response);
    }
    
}