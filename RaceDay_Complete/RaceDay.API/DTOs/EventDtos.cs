using System.ComponentModel.DataAnnotations;
namespace RaceDay.API.DTOs;
public class CreateEventRequest
{
    [Required, StringLength(150, MinimumLength = 2)] public string EventName { get; set; } = string.Empty;
    [Required] public DateTime EventDate { get; set; }
    [Required, StringLength(150, MinimumLength = 2)] public string Location { get; set; } = string.Empty;
    [Required, StringLength(50)] public string Distance { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int CategoryID { get; set; }
    [Range(1, 1000000)] public int MaxParticipants { get; set; } = 100;
    [StringLength(500)] public string? RouteDescription { get; set; }
}
public class UpdateEventRequest
{
    [Required, StringLength(150, MinimumLength = 2)] public string EventName { get; set; } = string.Empty;
    [Required] public DateTime EventDate { get; set; }
    [Required, StringLength(150, MinimumLength = 2)] public string Location { get; set; } = string.Empty;
    [Required, StringLength(50)] public string Distance { get; set; } = string.Empty;
    [Required] public string Status { get; set; } = "Upcoming";
}
public record EventResponse(int EventId, string EventName, DateTime EventDate, string Location, string? RouteDescription, string Distance, string Status, int MaxParticipants, int CategoryId, string CategoryName, int OrganizerId, string OrganizerName, int ConfirmedEnrolments);
public record EnrolmentResponse(int EnrolmentId, int EventId, string EventName, int UserId, string ParticipantName, DateTime EnrolmentDate, string Status);
public class ResultRequest
{
    [Required, StringLength(20, MinimumLength = 1)] public string FinishTime { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int? PositionOverall { get; set; }
    [Range(1, int.MaxValue)] public int? PositionCategory { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
}
public record ResultResponse(int ResultId, int EnrolmentId, int EventId, string EventName, int UserId, string ParticipantName, string FinishTime, int? PositionOverall, int? PositionCategory, string? Notes);
