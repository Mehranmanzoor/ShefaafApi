using System;

namespace ShefaafAPI.Models.DTOs;

public class AddToCartRequest
{
    public required string Email { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
