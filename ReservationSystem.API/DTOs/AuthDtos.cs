using System;
using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.API.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

    public string Role { get; set; } = "User";
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public DateTime Expiration { get; set; }
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}
