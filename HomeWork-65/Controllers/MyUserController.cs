using HomeWork_65.Models;
using HomeWork_65.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeWork_65.Controllers;

[Authorize]
public class MyUserController : Controller
{
    private MyUserContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<MyUser> _userManager;
    private readonly SignInManager<MyUser> _signInManager;

    public MyUserController(UserManager<MyUser> userManager, SignInManager<MyUser> signInManager, MyUserContext context,IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _env = env;
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

    public IActionResult Block(int? id)
    {
        if (id.HasValue)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            user.IsBlocked = true;
            _context.SaveChanges();
            return RedirectToAction("AllUsers", "Chat");
        }
        return NotFound();
    }
    [Authorize(Roles = "admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async  Task<IActionResult> Create(RegisterViewModel model,IFormFile file)
    {
        if (ModelState.IsValid)
        {
            if (_userManager.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Пользователь с таким Email уже существует.");
            }
            if (_userManager.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Пользователь с таким именем уже существует.");
            }
            else
            {
                string? relativePath = null;
                string uploadedFileName = Path.Combine(_env.WebRootPath, "images/avatars");
                if (!Directory.Exists(uploadedFileName))
                {
                    Directory.CreateDirectory(uploadedFileName);
                }
            
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            
                string filesavePath = Path.Combine(uploadedFileName, fileName);
                using (var stream = new FileStream(filesavePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                relativePath = Path.Combine("images/avatars", fileName.Replace("\\", "/"));
                MyUser user = new MyUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Avatar = relativePath,
                    Age = model.Age,
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "user");
                    return RedirectToAction("AllUsers", "Chat");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            
        }
        return View(model);
    }
}