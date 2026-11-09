using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private static readonly string[] Allowed = ["image/jpeg", "image/png", "image/webp"];
    public MediaController(AppDbContext db, IConfiguration configuration)
    { _db = db; _configuration = configuration; }

    private async Task<string> Store(IFormFile file, string folder)
    {
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Image must be between 1 byte and 5 MB.");
        if (!Allowed.Contains(file.ContentType.ToLowerInvariant())) throw new InvalidOperationException("Only JPG, PNG and WebP images are accepted.");
        var connection = _configuration.GetConnectionString("AzureBlobStorage") ?? _configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage:ConnectionString.");
        var containerName = _configuration["AzureBlobStorage:ContainerName"] ?? "raceday-media";
        var service = new BlobServiceClient(connection);
        var container = service.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None);
        var ext = file.ContentType switch { "image/jpeg" => ".jpg", "image/png" => ".png", _ => ".webp" };
        var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{ext}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } });
        // Prefer a short-lived SAS URL when the connection string includes an account key.
        if (blob.CanGenerateSasUri)
        {
            var sas = new BlobSasBuilder { BlobContainerName = containerName, BlobName = blob.Name, Resource = "b", ExpiresOn = DateTimeOffset.UtcNow.AddDays(365) };
            sas.SetPermissions(BlobSasPermissions.Read);
            return blob.GenerateSasUri(sas).ToString();
        }
        return blob.Uri.ToString();
    }

    [HttpPost("events/{eventId:int}/banner")]
    [Authorize(Roles = AppRoles.Organiser)]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(int eventId, IFormFile file)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();
        try { ev.BannerImageUrl = await Store(file, "event-banners"); await _db.SaveChangesAsync(); return Ok(new { imageUrl = ev.BannerImageUrl }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("profile/picture")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == User.GetUserId());
        if (user is null) return NotFound(new { message = "User not found." });
        try { user.ProfilePictureUrl = await Store(file, "profile-pictures"); await _db.SaveChangesAsync(); return Ok(new { imageUrl = user.ProfilePictureUrl }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
