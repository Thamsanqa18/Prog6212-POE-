using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db) => _db = db;

    /// <summary>Browse events (Organiser and Participant). Optionally filter by type or upcoming events only.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents([FromQuery] EventType? type, [FromQuery] bool upcomingOnly = false)
    {
        IQueryable<RaceEvent> query = _db.Events.AsNoTracking();

        if (User.Identity?.IsAuthenticated == true && User.IsInRole(AppRoles.Organiser))
            query = query.Where(e => e.OrganiserId == User.GetUserId());

        if (type.HasValue)
            query = query.Where(e => e.EventType == type.Value);
        if (upcomingOnly)
            query = query.Where(e => e.EventDate >= DateTime.UtcNow);

        var events = await query.OrderBy(e => e.EventDate).Select(Mappings.ToEventDto).ToListAsync();
        return Ok(events);
    }

    /// <summary>View a single event (Organiser and Participant).</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var dto = await _db.Events.AsNoTracking().Where(e => e.EventId == id).Select(Mappings.ToEventDto).FirstOrDefaultAsync();
        return dto is null ? NotFound(new ApiError("Event not found.")) : Ok(dto);
    }

    /// <summary>Create an event (Organiser only).</summary>
    /// <remarks>Expects name, description, event date, location, distance in km and event type (Run, Walk or Cycle).</remarks>
    [HttpPost]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventDto>> CreateEvent(EventRequestDto dto)
    {
        var ev = new RaceEvent
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            EventDate = dto.EventDate,
            Location = dto.Location.Trim(),
            DistanceKm = dto.DistanceKm,
            EventType = dto.EventType,
            OrganiserId = User.GetUserId()
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        var created = await _db.Events.AsNoTracking().Where(e => e.EventId == ev.EventId).Select(Mappings.ToEventDto).FirstAsync();
        return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, created);
    }

    /// <summary>Update an event (Organiser who created the event only).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, EventRequestDto dto)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();

        ev.Name = dto.Name.Trim();
        ev.Description = dto.Description.Trim();
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location.Trim();
        ev.DistanceKm = dto.DistanceKm;
        ev.EventType = dto.EventType;
        await _db.SaveChangesAsync();

        var updated = await _db.Events.AsNoTracking().Where(e => e.EventId == id).Select(Mappings.ToEventDto).FirstAsync();
        return Ok(updated);
    }

    /// <summary>Delete an event together with its categories, enrolments and results (Organiser who created the event only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();

        var enrolments = await _db.Enrolments.Where(x => x.EventId == id).ToListAsync();
        var enrolmentIds = enrolments.Select(x => x.EnrolmentId).ToList();
        var results = await _db.Results.Where(r => enrolmentIds.Contains(r.EnrolmentId)).ToListAsync();
        var categories = await _db.Categories.Where(c => c.EventId == id).ToListAsync();

        _db.Results.RemoveRange(results);
        _db.Enrolments.RemoveRange(enrolments);
        _db.Categories.RemoveRange(categories);
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
