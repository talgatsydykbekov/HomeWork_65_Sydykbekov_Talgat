using HomeWork_65.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeWork_65.Controllers;

public class ChatController : Controller
{
    private readonly UserManager<MyUser> _userManager;
    private readonly SignInManager<MyUser> _signInManager;

    public ChatController(UserManager<MyUser> userManager, SignInManager<MyUser> signInManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    // GET
    public IActionResult Index()
    {
        return View();
    }
}