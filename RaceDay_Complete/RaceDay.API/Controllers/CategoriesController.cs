using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(RaceDayDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll() =>
        Ok(await db.Categories.AsNoTracking().OrderBy(c => c.CategoryName).Select(c => new CategoryResponse(c.CategoryId, c.CategoryName, c.Description)).ToListAsync());

    [HttpPost, Authorize(Roles = Roles.Organiser)]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request)
    {
        var name = request.CategoryName.Trim();
        if (await db.Categories.AnyAsync(c => c.CategoryName == name)) return Conflict(new { message = "Category name already exists." });
        var category = new Category { CategoryName = name, Description = request.Description?.Trim() };
        db.Categories.Add(category); await db.SaveChangesAsync();
        var response = new CategoryResponse(category.CategoryId, category.CategoryName, category.Description);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id); if (category is null) return NotFound();
        var name = request.CategoryName.Trim();
        if (await db.Categories.AnyAsync(c => c.CategoryId != id && c.CategoryName == name)) return Conflict(new { message = "Category name already exists." });
        category.CategoryName = name; category.Description = request.Description?.Trim(); await db.SaveChangesAsync();
        return Ok(new CategoryResponse(category.CategoryId, category.CategoryName, category.Description));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id); if (category is null) return NotFound();
        if (await db.Events.AnyAsync(e => e.CategoryId == id) || await db.EventCategories.AnyAsync(ec => ec.CategoryId == id))
            return Conflict(new { message = "Cannot delete a category used by an event." });
        db.Categories.Remove(category); await db.SaveChangesAsync(); return Ok(new { message = "Category deleted." });
    }
}
