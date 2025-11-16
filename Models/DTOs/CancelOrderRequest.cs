using System;

namespace ShefaafAPI.Models.DTOs;

public class CancelOrderRequest
{
    public required string Email { get; set; }
    public required string CancellationReason { get; set; }
}
