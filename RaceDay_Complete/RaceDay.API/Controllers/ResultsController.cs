using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/enrolments/{id:int}/result")]
public class ResultsController(RaceDayDbContext db) : ControllerBase
{
    [HttpPost, Authorize(Roles = Roles.Organiser)]
    public async Task<ActionResult<ResultResponse>> Create(int id, ResultRequest request)
    {
        var enrolment = await db.Enrolments.Include(e => e.Event).Include(e => e.User).SingleOrDefaultAsync(e => e.EnrolmentId == id);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });
        if (await db.Results.AnyAsync(r => r.EnrolmentId == id)) return Conflict(new { message = "A result already exists for this enrolment." });
        var result = new Result { EnrolmentId = id, FinishTime = request.FinishTime.Trim(), PositionOverall = request.PositionOverall,
            PositionCategory = request.PositionCategory, Notes = request.Notes?.Trim() };
        db.Results.Add(result); enrolment.Status = EnrolmentStatuses.Completed; await db.SaveChangesAsync();
        return StatusCode(201, ToResponse(result, enrolment));
    }

    [HttpPut, Authorize(Roles = Roles.Organiser)]
    public async Task<ActionResult<ResultResponse>> Update(int id, ResultRequest request)
    {
        var result = await db.Results.Include(r => r.Enrolment!).ThenInclude(e => e.Event).Include(r => r.Enrolment!).ThenInclude(e => e.User)
            .SingleOrDefaultAsync(r => r.EnrolmentId == id);
        if (result is null) return NotFound();
        result.FinishTime = request.FinishTime.Trim(); result.PositionOverall = request.PositionOverall;
        result.PositionCategory = request.PositionCategory; result.Notes = request.Notes?.Trim(); await db.SaveChangesAsync();
        return Ok(ToResponse(result, result.Enrolment!));
    }

    private static ResultResponse ToResponse(Result r, Enrolment e) => new(r.ResultId, r.EnrolmentId, e.EventId, e.Event?.EventName ?? "", e.UserId,
        e.User is null ? "" : e.User.FirstName + " " + e.User.LastName, r.FinishTime, r.PositionOverall, r.PositionCategory, r.Notes);
}
