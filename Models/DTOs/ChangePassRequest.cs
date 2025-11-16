using System;

namespace ShefaafAPI.Models.DTOs;

public class ChangePassRequest
{
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}
