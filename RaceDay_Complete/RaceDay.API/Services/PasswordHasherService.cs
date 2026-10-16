using Microsoft.AspNetCore.Identity;
using RaceDay.API.Models;
namespace RaceDay.API.Services;
public interface IPasswordHasherService
{
    string Hash(User user, string password);
    bool Verify(User user, string hashedPassword, string providedPassword);
}
public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();
    public string Hash(User user, string password) => _hasher.HashPassword(user, password);
    public bool Verify(User user, string hashedPassword, string providedPassword) =>
        _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) != PasswordVerificationResult.Failed;
}
