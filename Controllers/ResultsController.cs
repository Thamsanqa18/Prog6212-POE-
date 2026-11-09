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
public class ResultsController : ControllerBase
{
    private const string TimeFormat = @"hh\:mm\:ss";
    private readonly AppDbContext _db;

    public ResultsController(AppDbContext db) => _db = db;

    private IQueryable<RaceResult> ResultsWithDetails() =>
        _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.User)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category);

    private static bool TryParseFinishTime(string value, out TimeSpan time) =>
        TimeSpan.TryParseExact(value?.Trim(), TimeFormat, null, out time) && time > TimeSpan.Zero;

    /// <summary>Capture a participant's finish time and finishing position for an event (Organiser who created the event only).</summary>
    /// <remarks>
    /// Expects the enrolment id (the participant's entry into this event), the finish time as hh:mm:ss and the finishing position.
    /// Each enrolment can have one result and each finishing position can be used once per event.
    /// </remarks>
    [HttpPost("events/{eventId:int}/results")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResultDto>> CaptureResult(int eventId, CaptureResultDto dto)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();
        if (!TryParseFinishTime(dto.FinishTime, out var finishTime))
            return BadRequest(new ApiError("Finish time must be in hh:mm:ss format and greater than zero."));

        var enrolmentExists = await _db.Enrolments.AnyAsync(x => x.EnrolmentId == dto.EnrolmentId && x.EventId == eventId);
        if (!enrolmentExists)
            return BadRequest(new ApiError("This enrolment does not belong to the event."));

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
            return Conflict(new ApiError("A result has already been captured for this participant. Use PUT to change it."));

        if (await _db.Results.AnyAsync(r => r.Enrolment.EventId == eventId && r.FinishPosition == dto.FinishPosition))
            return Conflict(new ApiError("Another participant already has this finishing position in this event."));

        var result = new RaceResult { EnrolmentId = dto.EnrolmentId, FinishTime = finishTime, FinishPosition = dto.FinishPosition };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var created = await ResultsWithDetails().AsNoTracking().FirstAsync(r => r.ResultId == result.ResultId);
        return CreatedAtAction(nameof(GetEventResults), new { eventId }, Mappings.ToResultDto(created));
    }

    /// <summary>Correct a captured result (Organiser who created the event only).</summary>
    [HttpPut("results/{resultId:int}")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResultDto>> UpdateResult(int resultId, UpdateResultDto dto)
    {
        var result = await ResultsWithDetails().FirstOrDefaultAsync(r => r.ResultId == resultId);
        if (result is null)
            return NotFound(new ApiError("Result not found."));
        if (result.Enrolment.Event.OrganiserId != User.GetUserId())
            return Forbid();

        if (!TryParseFinishTime(dto.FinishTime, out var finishTime))
            return BadRequest(new ApiError("Finish time must be in hh:mm:ss format and greater than zero."));

        var eventId = result.Enrolment.EventId;
        if (await _db.Results.AnyAsync(r => r.ResultId != resultId && r.Enrolment.EventId == eventId && r.FinishPosition == dto.FinishPosition))
            return Conflict(new ApiError("Another participant already has this finishing position in this event."));

        result.FinishTime = finishTime;
        result.FinishPosition = dto.FinishPosition;
        await _db.SaveChangesAsync();

        return Ok(Mappings.ToResultDto(result));
    }

    /// <summary>View all results captured for an event, ordered by finishing position (Organiser who created the event only).</summary>
    [HttpGet("events/{eventId:int}/results")]
    [Authorize(Roles = AppRoles.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetEventResults(int eventId)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null)
            return NotFound(new ApiError("Event not found."));
        if (ev.OrganiserId != User.GetUserId())
            return Forbid();

        var results = await ResultsWithDetails().AsNoTracking()
            .Where(r => r.Enrolment.EventId == eventId)
            .OrderBy(r => r.FinishPosition)
            .ToListAsync();
        return Ok(results.Select(Mappings.ToResultDto));
    }

    /// <summary>View the logged-in participant's own results across all events (Participant only).</summary>
    [HttpGet("results/mine")]
    [Authorize(Roles = AppRoles.Participant)]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetMyResults()
    {
        var userId = User.GetUserId();
        var results = await ResultsWithDetails().AsNoTracking()
            .Where(r => r.Enrolment.UserId == userId)
            .OrderByDescending(r => r.Enrolment.Event.EventDate)
            .ToListAsync();
        var mapped = results.Select(Mappings.ToResultDto).ToList();
        foreach (var item in mapped)
            item.TotalFinishers = await _db.Results.CountAsync(r => r.Enrolment.EventId == item.EventId);
        return Ok(mapped);
    }
}
