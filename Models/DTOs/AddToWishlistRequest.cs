using System;

namespace ShefaafAPI.Models.DTOs;

public class AddToWishlistRequest
{
    public required string Email { get; set; }
    public Guid ProductId { get; set; }
}
