namespace RaceDay.API.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
