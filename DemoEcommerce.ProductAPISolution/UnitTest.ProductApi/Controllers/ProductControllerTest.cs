using eCommerce.SharedLibrary.Responses;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Presentation.Controllers;

namespace UnitTest.ProductApi.Controllers;

public class ProductControllerTest
{
    private readonly IProduct productInterface;
    private readonly ProductsController productsController;

    public ProductControllerTest()
    {
        // setup dependencies
        productInterface = A.Fake<IProduct>();
        
        // setup system under test - SUT
        productsController = new ProductsController(productInterface);


    }
    
    // GET ALL PRODUCTS
    [Fact]
    public async Task GetProduct_WhenProductExists_ReturnOkResponseWithProducts()
    {
        // Arrange 
        var products = new List<Product>()
        {
            new() { Id = 1, Name = "Product 1", Quantity = 10, Price = 100.30m },
            new() { Id = 2, Name = "Product 2", Quantity = 110, Price = 1010.30m }
        };
        
        // setup fake response for GetAllAsync method
        A.CallTo(() => productInterface.GetAllAsync()).Returns(products);
        
        // Act
        var result = await productsController.GetProducts();
        
        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var returnedProducts = okResult.Value as IEnumerable<ProductDTO>;
        returnedProducts!.Should().NotBeNull();
        returnedProducts!.Should().HaveCount(2);
        returnedProducts!.First().Id.Should().Be(1);
        returnedProducts!.Last().Id.Should().Be(2);
        

    }

    [Fact]
    public async Task GetProduct_WhenNoProductsExist_ReturnNotFoundResponse()
    {
        // Arrange
        var products = new List<Product>();
        
        // Setup fake response for GetAllAsync()
        A.CallTo(() => productInterface.GetAllAsync()).Returns(products);

        // Act
        var result = await productsController.GetProducts();
        
        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var message = notFoundResult.Value as string;
        message.Should().Be("No products detected in the database");

    }

    // CREATE PRODUCT
    [Fact]
    public async Task CreateProduct_WhenModelStateIsInvaldi_ReturnBadRequest()
    {
        // Arrange 
        var productDTO = new ProductDTO(1, "Product 1", 34, 69.75m);
        productsController.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await productsController.CreateProduct(productDTO);

        // Assert
        var badRequestresult = result.Result as BadRequestObjectResult;
        badRequestresult.Should().NotBeNull();
        badRequestresult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

    }

    [Fact]
    public async Task CreateProduct_WhenCreateIsSuccessfull_ReturnOkResponse()
    {
        // Arrange
        var productDto = new ProductDTO(1, "Product 1", 34, 49.75m);
        var response = new Response(true, "Created");
        // Act
        A.CallTo(() => productInterface.CreateAsync(A<Product>.Ignored)).Returns(response);
        var result = await productsController.CreateProduct(productDto);
        
        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult!.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var responseResult = okResult.Value as Response;
        responseResult!.Message.Should().Be("Created");
        responseResult!.Flag.Should().BeTrue();

    }

    [Fact]
    public async Task CreateProduct_WhenCreateFails_ReturnBadRequestResponse()
    {
        // Arrange
        var productDto = new ProductDTO(1, "Product 1", 34, 27.95m);
        var response = new Response(false, "Failed");

        // Act
        A.CallTo(() => productInterface.CreateAsync(A<Product>.Ignored)).Returns(response);
        var result = await productsController.CreateProduct(productDto);
        
        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var responseResult = badRequestResult.Value as Response;
        responseResult!.Should().NotBeNull();
        responseResult!.Message.Should().Be("Failed");
        responseResult!.Flag.Should().BeFalse();

    }
    
    // UPDATE PRODUCT
    [Fact]
    public async Task UpdateProduct_WhenUpdateIsSuccessful_ReturnOkResponse()
    {
        // Arrang
        var productDto = new ProductDTO(1, "Product 1", 34, 27.95m);
        var response = new Response(true, "Updated");

        // Act
        A.CallTo(() => productInterface.UpdateAsync(A<Product>.Ignored)).Returns(response);
        var result = await productsController.UpdateProduct(productDto);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult!.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var responseResult = okResult.Value as Response;
        responseResult!.Message.Should().Be("Updated");
        responseResult!.Flag.Should().BeTrue();
        
    }

    [Fact]
    public async Task UpdateProduct_WhenUpdateFails_ReturnBadRequestResponse()
    {
        // Arrang
        var productDto = new ProductDTO(1, "Product 1", 34, 27.95m);
        var response = new Response(false, "Update Failed");
        
        // Act
        A.CallTo(() => productInterface.UpdateAsync(A<Product>.Ignored)).Returns(response);
        var result = await productsController.UpdateProduct(productDto);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var responseResult = badRequestResult.Value as Response;
        responseResult!.Should().NotBeNull();
        responseResult!.Message.Should().Be("Update Failed");
        responseResult!.Flag.Should().BeFalse();

    }

    // DELETE PRODUCT
    [Fact]
    public async Task DeleteProduct_WhenDeleteIsSuccessful_ReturnOkResponse()
    {
        // Arrang
        var productDto = new ProductDTO(1, "Product 1", 34, 27.95m);
        var response = new Response(true, "Deleted Successfully");
        A.CallTo(() => productInterface.DeleteAsync(A<Product>.Ignored)).Returns(response);     
           
        // Act
        var result = await productsController.DeleteProduct(productDto);
        
        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult!.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var responseResult = okResult.Value as Response;
        responseResult!.Message.Should().Be("Deleted Successfully");
        responseResult!.Flag.Should().BeTrue();
        
    }

    [Fact]
    public async Task DeleteProduct_WhenDeleteFails_ReturnBadRequestResponse()
    {
        // Arrange
        var productDto = new ProductDTO(1, "Product 1", 34, 27.95m);
        var response = new Response(false, "Deleted Failed");
        A.CallTo(() => productInterface.DeleteAsync(A<Product>.Ignored)).Returns(response);     
        
        // Act
        var result = await productsController.DeleteProduct(productDto);
        
        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var responseResult = badRequestResult.Value as Response;
        responseResult!.Should().NotBeNull();
        responseResult!.Message.Should().Be("Deleted Failed");
        responseResult!.Flag.Should().BeFalse();
        
    }
    
}   