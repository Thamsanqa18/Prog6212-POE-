using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize(Roles = Roles.Participant)]
public class EnrolmentsController(RaceDayDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await db.Enrolments.SingleOrDefaultAsync(e => e.EnrolmentId == id);
        if (enrolment is null) return NotFound();
        if (enrolment.UserId != CurrentUserId) return Forbid();
        if (enrolment.Status == EnrolmentStatuses.Cancelled) return Conflict(new { message = "Enrolment is already cancelled." });
        enrolment.Status = EnrolmentStatuses.Cancelled; await db.SaveChangesAsync();
        return Ok(new { message = "Enrolment cancelled." });
    }

}
