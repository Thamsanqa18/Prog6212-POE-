using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.DTOs;

public class EventRequestDto
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    /// <summary>Distance in kilometres (0.1 to 1000).</summary>
    [Range(0.1, 1000)]
    public decimal DistanceKm { get; set; }

    /// <summary>Run, Walk or Cycle.</summary>
    [EnumDataType(typeof(EventType))]
    public EventType EventType { get; set; }
}

public class EventDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public int OrganiserId { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }
}

public class CategoryRequestDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Age or Distance.</summary>
    [EnumDataType(typeof(CategoryType))]
    public CategoryType CategoryType { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
}

public class EnrolRequestDto
{
    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}

public class CaptureResultDto
{
    [Range(1, int.MaxValue)]
    public int EnrolmentId { get; set; }

    /// <summary>Finish time in hh:mm:ss format, e.g. 01:45:30.</summary>
    [Required]
    public string FinishTime { get; set; } = string.Empty;

    /// <summary>Overall finishing position (1 or higher). Must be unique within the event.</summary>
    [Range(1, int.MaxValue)]
    public int FinishPosition { get; set; }
}

public class UpdateResultDto
{
    /// <summary>Finish time in hh:mm:ss format, e.g. 01:45:30.</summary>
    [Required]
    public string FinishTime { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int FinishPosition { get; set; }
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string FinishTime { get; set; } = string.Empty;
    public int FinishPosition { get; set; }
    public int TotalFinishers { get; set; }
}
