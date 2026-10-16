using System.ComponentModel.DataAnnotations;
namespace RaceDay.API.DTOs;
public class CategoryRequest
{
    [Required, StringLength(100, MinimumLength = 2)] public string CategoryName { get; set; } = string.Empty;
    [StringLength(255)] public string? Description { get; set; }
}
public record CategoryResponse(int CategoryId, string CategoryName, string? Description);
