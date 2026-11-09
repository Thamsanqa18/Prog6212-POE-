using System.Linq.Expressions;
using RaceDay.Api.Models;

namespace RaceDay.Api.DTOs;

public static class Mappings
{
    public static readonly Expression<Func<RaceEvent, EventDto>> ToEventDto = e => new EventDto
    {
        EventId = e.EventId,
        Name = e.Name,
        Description = e.Description,
        EventDate = e.EventDate,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        OrganiserId = e.OrganiserId,
        OrganiserName = e.Organiser.FirstName + " " + e.Organiser.LastName,
        BannerImageUrl = e.BannerImageUrl
    };

    public static readonly Expression<Func<EventCategory, CategoryDto>> ToCategoryDto = c => new CategoryDto
    {
        CategoryId = c.CategoryId,
        EventId = c.EventId,
        Name = c.Name,
        CategoryType = c.CategoryType
    };

    public static readonly Expression<Func<Enrolment, EnrolmentDto>> ToEnrolmentDto = x => new EnrolmentDto
    {
        EnrolmentId = x.EnrolmentId,
        EventId = x.EventId,
        EventName = x.Event.Name,
        EventDate = x.Event.EventDate,
        CategoryId = x.CategoryId,
        CategoryName = x.Category.Name,
        ParticipantId = x.UserId,
        ParticipantName = x.User.FirstName + " " + x.User.LastName,
        EnrolledAt = x.EnrolledAt
    };

    public static UserDto ToUserDto(AppUser u) => new()
    {
        UserId = u.UserId,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Role = u.Role,
        PhoneNumber = u.PhoneNumber,
        DateOfBirth = u.DateOfBirth,
        ProfilePictureUrl = u.ProfilePictureUrl
    };

    /// <summary>The result's Enrolment (with Event, User and Category) must be loaded.</summary>
    public static ResultDto ToResultDto(RaceResult r) => new()
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        EventId = r.Enrolment.EventId,
        EventName = r.Enrolment.Event.Name,
        EventDate = r.Enrolment.Event.EventDate,
        ParticipantName = r.Enrolment.User.FirstName + " " + r.Enrolment.User.LastName,
        CategoryName = r.Enrolment.Category.Name,
        FinishTime = r.FinishTime.ToString(@"hh\:mm\:ss"),
        FinishPosition = r.FinishPosition
    };
}
