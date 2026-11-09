using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EnrolmentsController(AppDbContext db) => _db = db;

    /// <summary>Enter an event by selecting a category (Participant only).</summary>
    /// <remarks>
    /// Expects the event id and the id of one of that event's categories. A participant can enter an event once,
    /// and cannot enter an event that has already taken place.
    /// </remarks>
    [HttpPost("enrolments")]
    [Authorize(Roles = AppRoles.Participant)]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnrolmentDto>> Enrol(EnrolRequestDto dto)
    {
        var userId = User.GetUserId();

        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == dto.EventId);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));

        if (ev.EventDate <= DateTime.UtcNow)
            return BadRequest(new ApiError("This event has already taken place, so enrolment is closed."));

        var categoryBelongsToEvent = await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId && c.EventId == dto.EventId);
        if (!categoryBelongsToEvent)
            return BadRequest(new ApiError("The selected category does not belong to this event."));

        if (await _db.Enrolments.AnyAsync(x => x.UserId == userId && x.EventId == dto.EventId))
            return Conflict(new ApiError("You have already entered this event."));

        var enrolment = new Enrolment { UserId = userId, EventId = dto.EventId, CategoryId = dto.CategoryId };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var created = await _db.Enrolments.AsNoTracking().Where(x => x.EnrolmentId == enrolment.EnrolmentId).Select(Mappings.ToEnrolmentDto).FirstAsync();
        return CreatedAtAction(nameof(GetMyEnrolments), null, created);
    }

    /// <summary>View the logged-in participant's own enrolments (Participant only).</summary>
    [HttpGet("enrolments/mine")]
    [Authorize(Roles = AppRoles.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetMyEnrolments()
    {
        var userId = User.GetUserId();
        var list = await _db.Enrolments.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Event.EventDate)
            .Select(Mappings.ToEnrolmentDto)
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>View all enrolments for an event (Organiser who created the event only).</summary>
    [HttpGet("events/{eventId:int}/enrolments")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetEventEnrolments(int eventId)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();

        var list = await _db.Enrolments.AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.EnrolledAt)
            .Select(Mappings.ToEnrolmentDto)
            .ToListAsync();
        return Ok(list);
    }
}
