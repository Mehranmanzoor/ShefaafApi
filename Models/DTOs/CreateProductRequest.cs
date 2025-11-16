using System;

namespace ShefaafAPI.Models.DTOs;

public class CreateProductRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Weight { get; set; }
}
