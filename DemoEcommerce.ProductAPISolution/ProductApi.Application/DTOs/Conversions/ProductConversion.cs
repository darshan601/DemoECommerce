using ProductApi.Domain.Entities;

namespace ProductApi.Application.DTOs.Conversions;

public static class ProductConversion
{
    public static Product ToEntity(ProductDTO productDto) => new()
    {
        Id = productDto.Id,
        Name = productDto.Name,
        Quantity = productDto.Quantity,
        Price = productDto.Price
    };

    public static (ProductDTO?, IEnumerable<ProductDTO>?) FromEntity(Product product, IEnumerable<Product>? products)
    {
        // return single
        if (product is not null || products is null)
        {
            var singleProduct = new ProductDTO
                (product!.Id, 
                product!.Name,
                product!.Quantity,
                product!.Price
                );

            return (singleProduct, null);
        }

        if (product is null || products is not null)
        {
            var _products = products.Select(p =>
            
                new ProductDTO(p.Id, p.Name, p.Quantity, p.Price)
            ).ToList();

            return (null, _products);
        }

        return (null, null);
    }
}