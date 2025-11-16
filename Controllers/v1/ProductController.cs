using Microsoft.AspNetCore.Mvc;
using ShefaafAPI.Models;
using ShefaafAPI.Models.DTOs;
using ShefaafAPI.Services;

namespace ShefaafAPI.Controllers.v1;

[Route("v1/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ISqlService _sqlService;

    public ProductController(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpGet("All")]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _sqlService.GetAllProducts();
            return Ok(new
            {
                success = true,
                count = products.Count,
                products
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        try
        {
            var product = await _sqlService.GetProductById(id);
            
            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                success = true,
                product
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpGet("Category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        try
        {
            var products = await _sqlService.GetProductsByCategory(category);
            return Ok(new
            {
                success = true,
                category,
                count = products.Count,
                products
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpGet("Search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Search term is required"
                });
            }

            var products = await _sqlService.SearchProducts(q);
            return Ok(new
            {
                success = true,
                searchTerm = q,
                count = products.Count,
                products
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest model)
    {
        try
        {
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Stock = model.Stock,
                ImageUrl = model.ImageUrl,
                Category = model.Category,
                Weight = model.Weight
            };

            await _sqlService.CreateProduct(product);

            return Ok(new
            {
                success = true,
                message = "Product created successfully",
                product
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductRequest model)
    {
        try
        {
            var product = await _sqlService.GetProductById(id);
            
            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found"
                });
            }

            // Update only provided fields
            if (model.Name != null) product.Name = model.Name;
            if (model.Description != null) product.Description = model.Description;
            if (model.Price.HasValue) product.Price = model.Price.Value;
            if (model.Stock.HasValue) product.Stock = model.Stock.Value;
            if (model.ImageUrl != null) product.ImageUrl = model.ImageUrl;
            if (model.Category != null) product.Category = model.Category;
            if (model.Weight != null) product.Weight = model.Weight;
            if (model.IsActive.HasValue) product.IsActive = model.IsActive.Value;

            var updated = await _sqlService.UpdateProduct(product);

            return Ok(new
            {
                success = true,
                message = "Product updated successfully",
                product
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            var deleted = await _sqlService.DeleteProduct(id);
            
            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Product deleted successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }

    [HttpPatch("UpdateStock/{id}")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int stock)
    {
        try
        {
            var updated = await _sqlService.UpdateProductStock(id, stock);
            
            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Stock updated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Server Error",
                error = ex.Message
            });
        }
    }
}
