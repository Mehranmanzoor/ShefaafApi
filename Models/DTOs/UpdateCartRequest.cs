using System;

namespace ShefaafAPI.Models.DTOs;

public class UpdateCartRequest
{
    public required string Email { get; set; }
    public Guid CartId { get; set; }
    public int Quantity { get; set; }
}
