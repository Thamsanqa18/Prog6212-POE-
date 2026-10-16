namespace RaceDay.API.Models;

public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string FinishTime { get; set; } = string.Empty;
    public int? PositionOverall { get; set; }
    public int? PositionCategory { get; set; }
    public string? Notes { get; set; }
    public Enrolment? Enrolment { get; set; }
}
