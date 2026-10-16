using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static IQueryable<Event> WithDetails(IQueryable<Event> query) => query.Include(e => e.Category).Include(e => e.Organizer).Include(e => e.Enrolments);
    private static EventResponse ToResponse(Event e) => new(e.EventId, e.EventName, e.EventDate, e.Location, e.RouteDescription, e.Distance, e.Status, e.MaxParticipants,
        e.CategoryId, e.Category?.CategoryName ?? "", e.OrganizerId, e.Organizer is null ? "" : e.Organizer.FirstName + " " + e.Organizer.LastName,
        e.Enrolments.Count(x => x.Status == EnrolmentStatuses.Confirmed));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll()
    {
        var events = await WithDetails(db.Events.AsNoTracking()).Where(e => e.EventDate >= DateTime.Today && e.Status != EventStatuses.Cancelled).OrderBy(e => e.EventDate).ToListAsync();
        return Ok(events.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventResponse>> GetById(int id)
    {
        var e = await WithDetails(db.Events.AsNoTracking()).SingleOrDefaultAsync(x => x.EventId == id);
        return e is null ? NotFound() : Ok(ToResponse(e));
    }

    [HttpPost, Authorize(Roles = Roles.Organiser)]
    public async Task<ActionResult<EventResponse>> Create(CreateEventRequest request)
    {
        if (request.EventDate.Date < DateTime.Today) return BadRequest(new { message = "Event date cannot be in the past." });
        var category = await db.Categories.FindAsync(request.CategoryID); if (category is null) return BadRequest(new { message = "CategoryID does not exist." });
        var e = new Event { OrganizerId = CurrentUserId, CategoryId = request.CategoryID, EventName = request.EventName.Trim(), EventDate = request.EventDate,
            Location = request.Location.Trim(), Distance = request.Distance.Trim(), MaxParticipants = request.MaxParticipants,
            RouteDescription = request.RouteDescription?.Trim(), Status = EventStatuses.Upcoming };
        db.Events.Add(e); await db.SaveChangesAsync();
        db.EventCategories.Add(new EventCategory { EventId = e.EventId, CategoryId = e.CategoryId }); await db.SaveChangesAsync();
        e = await WithDetails(db.Events).SingleAsync(x => x.EventId == e.EventId);
        return CreatedAtAction(nameof(GetById), new { id = e.EventId }, ToResponse(e));
    }

    [HttpPut("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<ActionResult<EventResponse>> Update(int id, UpdateEventRequest request)
    {
        var e = await db.Events.FindAsync(id); if (e is null) return NotFound();
        if (!EventStatuses.All.Contains(request.Status, StringComparer.OrdinalIgnoreCase)) return BadRequest(new { message = "Invalid event status." });
        if (request.EventDate.Date < DateTime.Today) return BadRequest(new { message = "Event date cannot be in the past." });
        e.EventName = request.EventName.Trim(); e.EventDate = request.EventDate; e.Location = request.Location.Trim(); e.Distance = request.Distance.Trim();
        e.Status = EventStatuses.All.Single(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        await db.SaveChangesAsync();
        e = await WithDetails(db.Events).SingleAsync(x => x.EventId == id); return Ok(ToResponse(e));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await db.Events.Include(x => x.Enrolments).SingleOrDefaultAsync(x => x.EventId == id); if (e is null) return NotFound();
        if (e.Enrolments.Any()) return Conflict(new { message = "Cannot delete an event that has enrolments. Cancel the event instead." });
        db.Events.Remove(e); await db.SaveChangesAsync(); return Ok(new { message = "Event deleted." });
    }

    [HttpPost("{id:int}/enrol"), Authorize(Roles = Roles.Participant)]
    public async Task<ActionResult<EnrolmentResponse>> Enrol(int id)
    {
        var e = await db.Events.Include(x => x.Enrolments).SingleOrDefaultAsync(x => x.EventId == id);
        if (e is null) return NotFound();
        if (e.Status != EventStatuses.Upcoming || e.EventDate.Date < DateTime.Today) return BadRequest(new { message = "This event is not open for enrolment." });
        if (!RegistrationRules.HasCapacity(e.Enrolments.Count(x => x.Status == EnrolmentStatuses.Confirmed), e.MaxParticipants)) return BadRequest(new { message = "Event is full." });
        var uid = CurrentUserId;
        var existing = await db.Enrolments.SingleOrDefaultAsync(x => x.EventId == id && x.UserId == uid);
        if (existing is not null && existing.Status != EnrolmentStatuses.Cancelled) return Conflict(new { message = "You are already enrolled in this event." });
        if (existing is not null) { existing.Status = EnrolmentStatuses.Confirmed; existing.EnrolmentDate = DateTime.UtcNow; await db.SaveChangesAsync();
            return StatusCode(201, new EnrolmentResponse(existing.EnrolmentId, id, e.EventName, uid, User.Identity?.Name ?? "Participant", existing.EnrolmentDate, existing.Status)); }
        var enrolment = new Enrolment { EventId = id, UserId = uid, EnrolmentDate = DateTime.UtcNow, Status = EnrolmentStatuses.Confirmed };
        db.Enrolments.Add(enrolment); await db.SaveChangesAsync();
        var user = await db.Users.FindAsync(uid);
        return StatusCode(201, new EnrolmentResponse(enrolment.EnrolmentId, id, e.EventName, uid, user!.FirstName + " " + user.LastName, enrolment.EnrolmentDate, enrolment.Status));
    }

    [HttpGet("{id:int}/enrolments"), Authorize(Roles = Roles.Organiser)]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetEventEnrolments(int id)
    {
        if (!await db.Events.AnyAsync(e => e.EventId == id)) return NotFound();
        var list = await db.Enrolments.AsNoTracking().Where(e => e.EventId == id).Select(e => new EnrolmentResponse(e.EnrolmentId, e.EventId,
            e.Event!.EventName, e.UserId, e.User!.FirstName + " " + e.User.LastName, e.EnrolmentDate, e.Status)).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}/results")]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> GetEventResults(int id)
    {
        if (!await db.Events.AnyAsync(e => e.EventId == id)) return NotFound();
        var list = await db.Results.AsNoTracking().Where(r => r.Enrolment!.EventId == id).OrderBy(r => r.PositionOverall)
            .Select(r => new ResultResponse(r.ResultId, r.EnrolmentId, r.Enrolment!.EventId, r.Enrolment.Event!.EventName, r.Enrolment.UserId,
                r.Enrolment.User!.FirstName + " " + r.Enrolment.User.LastName, r.FinishTime, r.PositionOverall, r.PositionCategory, r.Notes)).ToListAsync();
        return Ok(list);
    }
}
