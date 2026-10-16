using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class RegisterRequest
{
    [Required, StringLength(50, MinimumLength = 1)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(50, MinimumLength = 1)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8), StringLength(100)] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = "Participant";
    [Phone, StringLength(20)] public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public record UserResponse(int UserId, string FirstName, string LastName, string Email, string Role, string? PhoneNumber, DateTime DateRegistered);
public record LoginResponse(string Token, DateTime ExpiresAt, UserResponse User);
public class UpdateProfileRequest
{
    [Required, StringLength(50, MinimumLength = 1)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(50, MinimumLength = 1)] public string LastName { get; set; } = string.Empty;
    [Phone, StringLength(20)] public string? PhoneNumber { get; set; }
}
