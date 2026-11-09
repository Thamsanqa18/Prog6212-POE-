using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db) => _db = db;

    /// <summary>View the categories available for an event (Organiser and Participant).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
            return NotFound(new ApiError("Event not found."));

        var categories = await _db.Categories.AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.Name)
            .Select(Mappings.ToCategoryDto)
            .ToListAsync();
        return Ok(categories);
    }

    /// <summary>Define an age or distance category for an event (Organiser who created the event only).</summary>
    /// <remarks>Expects a name (e.g. "Under 20", "Senior", "10km", "21km") and a category type (Age or Distance).</remarks>
    [HttpPost]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(int eventId, CategoryRequestDto dto)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();

        var name = dto.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.EventId == eventId && c.Name == name))
            return Conflict(new ApiError("This event already has a category with that name."));

        var category = new EventCategory { EventId = eventId, Name = name, CategoryType = dto.CategoryType };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var created = await _db.Categories.AsNoTracking().Where(c => c.CategoryId == category.CategoryId).Select(Mappings.ToCategoryDto).FirstAsync();
        return CreatedAtAction(nameof(GetCategories), new { eventId }, created);
    }

    /// <summary>Rename or change the type of a category (Organiser who created the event only).</summary>
    [HttpPut("{categoryId:int}")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int eventId, int categoryId, CategoryRequestDto dto)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.EventId == eventId);
        if (category is null)
            return NotFound(new ApiError("Category not found."));

        var name = dto.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.EventId == eventId && c.Name == name && c.CategoryId != categoryId))
            return Conflict(new ApiError("This event already has a category with that name."));

        category.Name = name;
        category.CategoryType = dto.CategoryType;
        await _db.SaveChangesAsync();

        return Ok(new CategoryDto { CategoryId = category.CategoryId, EventId = category.EventId, Name = category.Name, CategoryType = category.CategoryType });
    }

    /// <summary>Delete a category that nobody has enrolled in yet (Organiser who created the event only).</summary>
    [HttpDelete("{categoryId:int}")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.EventId == eventId);
        if (category is null)
            return NotFound(new ApiError("Category not found."));

        if (await _db.Enrolments.AnyAsync(x => x.CategoryId == categoryId))
            return Conflict(new ApiError("This category cannot be deleted because participants have already enrolled in it."));

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
