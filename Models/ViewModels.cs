using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Mvc.Models;
public class RegisterVm {
 [Required,Display(Name="First name")] public string FirstName {get;set;}="";
 [Required,Display(Name="Last name")] public string LastName {get;set;}="";
 [Required,EmailAddress] public string Email {get;set;}="";
 [Required,MinLength(8),DataType(DataType.Password)] public string Password {get;set;}="";
 [Required] public string Role {get;set;}="Participant";
 [Display(Name="Organiser access code")] public string? OrganiserCode {get;set;}
}
public class LoginVm { [Required,EmailAddress] public string Email {get;set;}=""; [Required,DataType(DataType.Password)] public string Password {get;set;}=""; }
public class EventVm {
 public int EventId {get;set;}
 [Required,StringLength(150)] public string Name {get;set;}="";
 [Required,StringLength(1000)] public string Description {get;set;}="";
 [Required,Display(Name="Event date and time"),DataType(DataType.DateTime)] public DateTime EventDate {get;set;}=DateTime.Now.AddDays(30);
 [Required] public string Location {get;set;}="";
 [Range(0.1,1000),Display(Name="Distance (km)")] public decimal DistanceKm {get;set;}=10;
 [Required] public string EventType {get;set;}="Run";
 public IFormFile? BannerImage {get;set;}
 
}
public class CategoryVm { [Required] public string Name {get;set;}=""; public string CategoryType {get;set;}="Age"; }
public class EnrolVm { public int EventId {get;set;} public int CategoryId {get;set;} }
public class ResultVm { public int EnrolmentId {get;set;} [Required] public string FinishTime {get;set;}="00:45:00"; [Range(1,int.MaxValue)] public int FinishPosition {get;set;}=1; }
public class ProfileVm { [Required] public string FirstName {get;set;}=""; [Required] public string LastName {get;set;}=""; [Required,EmailAddress] public string Email {get;set;}=""; public string? PhoneNumber {get;set;} public DateOnly? DateOfBirth {get;set;} public IFormFile? ProfilePicture {get;set;} public string? ProfilePictureUrl { get; set; }}
public class ApiUser { public int UserId{get;set;} public string FirstName{get;set;}=""; public string LastName{get;set;}=""; public string Email{get;set;}=""; public string Role{get;set;}=""; public string? PhoneNumber{get;set;} public DateOnly? DateOfBirth{get;set;} public string? ProfilePictureUrl{get;set;} }
public class RaceEventDto { public int EventId{get;set;} public string Name{get;set;}=""; public string Description{get;set;}=""; public DateTime EventDate{get;set;} public string Location{get;set;}=""; public decimal DistanceKm{get;set;} public string EventType{get;set;}=""; public string OrganiserName{get;set;}=""; public string? BannerImageUrl{get;set;} }
public class CategoryDto { public int CategoryId{get;set;} public int EventId{get;set;} public string Name{get;set;}=""; public string CategoryType{get;set;}=""; }
public class EnrolmentDto { public int EnrolmentId{get;set;} public int EventId{get;set;} public string EventName{get;set;}=""; public DateTime EventDate{get;set;} public int CategoryId{get;set;} public string CategoryName{get;set;}=""; public int ParticipantId{get;set;} public string ParticipantName{get;set;}=""; public DateTime EnrolledAt{get;set;} }
public class ResultDto { public int ResultId{get;set;} public int EnrolmentId{get;set;} public int EventId{get;set;} public string EventName{get;set;}=""; public DateTime EventDate{get;set;} public string ParticipantName{get;set;}=""; public string CategoryName{get;set;}=""; public string FinishTime{get;set;}=""; public int FinishPosition{get;set;} public int TotalFinishers{get;set;} }
