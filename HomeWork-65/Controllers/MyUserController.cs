using HomeWork_65.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeWork_65.Controllers;

public class MyUserController : Controller
{
    private MyUserContext _context;
    private readonly UserManager<MyUser> _userManager;
    private readonly SignInManager<MyUser> _signInManager;

    public MyUserController(UserManager<MyUser> userManager, SignInManager<MyUser> signInManager, MyUserContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    public IActionResult Details(int? id)
    {
        if (id.HasValue)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        return NotFound();
    }

    public IActionResult Edit(int? id)
    {
        if (id.HasValue)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        return NotFound();
    }
}