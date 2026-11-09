using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Models;using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController:Controller{
 readonly ApiClient api;public ParticipantController(ApiClient a)=>api=a;bool Guard()=>HttpContext.Session.GetString("Role")=="Participant";
 public async Task<IActionResult> Index(){if(!Guard())return RedirectToAction("Login","Account");return View(await api.Get<List<EnrolmentDto>>("api/enrolments/mine")??new());}
 public async Task<IActionResult> Enrol(int id){if(!Guard())return Forbid();var e=await api.Get<RaceEventDto>($"api/events/{id}");if(e==null)return NotFound();ViewBag.Event=e;ViewBag.Categories=await api.Get<List<CategoryDto>>($"api/events/{id}/categories")??new();return View(new EnrolVm{EventId=id});}
 [HttpPost, ValidateAntiForgeryToken]public async Task<IActionResult>Enrol(EnrolVm vm){if(!Guard())return Forbid();var r=await api.Send("POST","api/enrolments",new{eventId=vm.EventId,categoryId=vm.CategoryId});if(!r.Ok){TempData["Error"]=r.Error;return RedirectToAction("Enrol",new{id=vm.EventId});}TempData["Success"]="You're enrolled. See you on race day!";return RedirectToAction("Index");}
 public async Task<IActionResult>Results(){if(!Guard())return Forbid();return View(await api.Get<List<ResultDto>>("api/results/mine")??new());}
}
