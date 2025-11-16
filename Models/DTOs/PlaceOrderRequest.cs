using System;

namespace ShefaafAPI.Models.DTOs;

public class PlaceOrderRequest
{
    public required string Email { get; set; }
    public required string ShippingAddress { get; set; }
    public required string City { get; set; }
    public required string PinCode { get; set; }
    public required string PhoneNumber { get; set; }
    public string? PaymentMethod { get; set; } = "COD";
}
