using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(RaceDayDbContext db, IConfiguration configuration, IPasswordHasherService passwordHasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        if (!RegistrationRules.IsAllowedRole(request.Role)) return BadRequest(new { message = "Role must be Organiser or Participant." });
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict(new { message = "Email is already registered." });
        var user = new User { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email,
            Role = RegistrationRules.NormaliseRole(request.Role), PhoneNumber = request.PhoneNumber?.Trim(), DateRegistered = DateTime.UtcNow };
        user.PasswordHash = passwordHasher.Hash(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null || !passwordHasher.Verify(user, user.PasswordHash, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpiryMinutes", 120));
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims,
            expires: expires, signingCredentials: credentials);
        return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, ToResponse(user)));
    }

    internal static UserResponse ToResponse(User user) => new(user.UserId, user.FirstName, user.LastName, user.Email, user.Role, user.PhoneNumber, user.DateRegistered);
}
