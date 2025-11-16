using System;

namespace ShefaafAPI.Models.DTOs;

public class UpdateOrderStatusRequest
{
    public required string Status { get; set; }
}
