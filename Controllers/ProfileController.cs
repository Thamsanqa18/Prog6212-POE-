using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Models;using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ProfileController:Controller{
 readonly ApiClient api;public ProfileController(ApiClient a)=>api=a;
 public async Task<IActionResult>Index(){if(string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))return RedirectToAction("Login","Account");var u=await api.Get<ApiUser>("api/profile");if(u==null)return RedirectToAction("Login","Account");return View(new ProfileVm{FirstName=u.FirstName,LastName=u.LastName,Email=u.Email,PhoneNumber=u.PhoneNumber,DateOfBirth=u.DateOfBirth,ProfilePictureUrl=u.ProfilePictureUrl});}
 [HttpPost, ValidateAntiForgeryToken]public async Task<IActionResult>Index(ProfileVm vm){if(!ModelState.IsValid)return View(vm);var r=await api.Send("PUT","api/profile",new{firstName=vm.FirstName,lastName=vm.LastName,phoneNumber=vm.PhoneNumber,dateOfBirth=vm.DateOfBirth});if(!r.Ok){ModelState.AddModelError("",r.Error);return View(vm);}if(vm.ProfilePicture!=null){var up=await api.Upload("api/profile/picture",vm.ProfilePicture);if(!up.Ok){ModelState.AddModelError("",up.Error);return View(vm);}}HttpContext.Session.SetString("FirstName",vm.FirstName);HttpContext.Session.SetString("LastName",vm.LastName);TempData["Success"]="Profile updated.";return RedirectToAction("Index");}
}
