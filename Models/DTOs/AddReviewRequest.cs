using System;

namespace ShefaafAPI.Models.DTOs;

public class AddReviewRequest
{
    public required string Email { get; set; }
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
