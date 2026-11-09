using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class AccountController:Controller{
 private readonly ApiClient api; public AccountController(ApiClient a)=>api=a;
 [HttpGet] public IActionResult Login()=>View(new LoginVm());
 [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginVm vm){if(!ModelState.IsValid)return View(vm);var r=await api.Login(vm.Email,vm.Password);if(!r.Ok){ModelState.AddModelError("",r.Error);return View(vm);}return RedirectToAction("Index","Home");}
 [HttpGet] public IActionResult Register()=>View(new RegisterVm());
 [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterVm vm){if(!ModelState.IsValid)return View(vm);var r=await api.Register(new{firstName=vm.FirstName,lastName=vm.LastName,email=vm.Email,password=vm.Password,role=vm.Role,organiserCode=vm.OrganiserCode});if(!r.Ok){ModelState.AddModelError("",r.Error);return View(vm);}TempData["Success"]="Account created. You can now sign in.";return RedirectToAction("Login");}
 public async Task<IActionResult> Logout(){await api.Logout();return RedirectToAction("Index","Home");}
}
