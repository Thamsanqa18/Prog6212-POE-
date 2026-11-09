using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Services;using RaceDay.Mvc.Models;
namespace RaceDay.Mvc.Controllers;
public class HomeController:Controller{
 private readonly ApiClient api;public HomeController(ApiClient a)=>api=a;
 public IActionResult Error()=>View();
 public async Task<IActionResult> Index(string? type){ViewBag.Type=type;var events=await api.Get<List<RaceEventDto>>("api/events?upcomingOnly=true")??new();if(!string.IsNullOrWhiteSpace(type))events=events.Where(e=>e.EventType.Equals(type,StringComparison.OrdinalIgnoreCase)).ToList();return View(events);}
 public async Task<IActionResult> Details(int id){var ev=await api.Get<RaceEventDto>($"api/events/{id}");if(ev==null)return NotFound();ViewBag.Categories=await api.Get<List<CategoryDto>>($"api/events/{id}/categories")??new();return View(ev);}
}
