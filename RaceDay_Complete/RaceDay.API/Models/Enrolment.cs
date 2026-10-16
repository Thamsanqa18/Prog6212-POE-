namespace RaceDay.API.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = EnrolmentStatuses.Confirmed;
    public User? User { get; set; }
    public Event? Event { get; set; }
    public Result? Result { get; set; }
}

public static class EnrolmentStatuses
{
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";
}
