namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;
    public RaceEvent Event { get; set; } = null!;
    public EventCategory Category { get; set; } = null!;
    public RaceResult? Result { get; set; }
}
