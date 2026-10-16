namespace RaceDay.API.Models;

public class Event
{
    public int EventId { get; set; }
    public int OrganizerId { get; set; }
    public int CategoryId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? RouteDescription { get; set; }
    public string Distance { get; set; } = string.Empty;
    public string Status { get; set; } = EventStatuses.Upcoming;
    public int MaxParticipants { get; set; } = 100;
    public User? Organizer { get; set; }
    public Category? Category { get; set; }
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public static class EventStatuses
{
    public const string Upcoming = "Upcoming";
    public const string Ongoing = "Ongoing";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public static readonly string[] All = [Upcoming, Ongoing, Completed, Cancelled];
}
