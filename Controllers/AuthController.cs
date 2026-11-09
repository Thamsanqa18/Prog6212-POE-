using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IPasswordService passwords, IConfiguration config)
    {
        _db = db;
        _passwords = passwords;
        _config = config;
    }

    /// <summary>Register a new account as an Organiser or a Participant.</summary>
    /// <remarks>
    /// Expects first name, last name, email, password (min 8 characters), role ("Organiser" or "Participant"),
    /// and optionally phone number and date of birth. Registering as an Organiser also requires the
    /// organiser code, so that nobody can simply give themselves organiser rights.
    /// Returns the created user (the password is never returned).
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        if (!AppRoles.TryNormalise(dto.Role, out var role))
            return BadRequest(new ApiError("Role must be either 'Organiser' or 'Participant'."));

        if (role == AppRoles.Organiser)
        {
            var expected = _config["Registration:OrganiserCode"];
            if (string.IsNullOrWhiteSpace(expected) || dto.OrganiserCode != expected)
                return StatusCode(StatusCodes.Status403Forbidden, new ApiError("A valid organiser code is required to register as an Organiser."));
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new ApiError("An account with this email address already exists."));

        var user = new AppUser
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            PasswordHash = _passwords.Hash(dto.Password),
            Role = role,
            PhoneNumber = dto.PhoneNumber?.Trim(),
            DateOfBirth = dto.DateOfBirth
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created("/api/profile", Mappings.ToUserDto(user));
    }

    /// <summary>Log in with email and password.</summary>
    /// <remarks>On success a session cookie is issued that holds the user's id and role for all subsequent requests.</remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !_passwords.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new ApiError("Invalid email or password."));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Ok(Mappings.ToUserDto(user));
    }

    /// <summary>Log out and end the current session.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
