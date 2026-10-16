using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("profile")]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        var user = await db.Users.FindAsync(CurrentUserId);
        return user is null ? NotFound() : Ok(AuthController.ToResponse(user));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserResponse>> UpdateProfile(UpdateProfileRequest request)
    {
        var user = await db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();
        user.FirstName = request.FirstName.Trim(); user.LastName = request.LastName.Trim(); user.PhoneNumber = request.PhoneNumber?.Trim();
        await db.SaveChangesAsync();
        return Ok(AuthController.ToResponse(user));
    }

    [HttpGet("enrolments"), Authorize(Roles = Roles.Participant)]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> MyEnrolments()
    {
        var list = await db.Enrolments.AsNoTracking().Where(e => e.UserId == CurrentUserId)
            .Select(e => new EnrolmentResponse(e.EnrolmentId, e.EventId, e.Event!.EventName, e.UserId,
                e.User!.FirstName + " " + e.User.LastName, e.EnrolmentDate, e.Status)).ToListAsync();
        return Ok(list);
    }

    [HttpGet("results"), Authorize(Roles = Roles.Participant)]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> MyResults()
    {
        var list = await db.Results.AsNoTracking().Where(r => r.Enrolment!.UserId == CurrentUserId)
            .Select(r => new ResultResponse(r.ResultId, r.EnrolmentId, r.Enrolment!.EventId, r.Enrolment.Event!.EventName,
                r.Enrolment.UserId, r.Enrolment.User!.FirstName + " " + r.Enrolment.User.LastName,
                r.FinishTime, r.PositionOverall, r.PositionCategory, r.Notes)).ToListAsync();
        return Ok(list);
    }
}
