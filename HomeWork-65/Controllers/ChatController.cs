using HomeWork_65.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeWork_65.Controllers;
[Authorize]
public class ChatController : Controller
{
    private readonly UserManager<MyUser> _userManager;
    private readonly SignInManager<MyUser> _signInManager;
    private readonly MyUserContext _context;

    public ChatController(UserManager<MyUser> userManager, SignInManager<MyUser> signInManager, IWebHostEnvironment env, MyUserContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    // GET
    public IActionResult Index()
    {
        var messages = _context.Messages.Take(30).Include(m => m.User).OrderByDescending(m => m.Date).ToList();
        return View(messages);
    }

    public IActionResult AllUsers()
    {
        var users = _userManager.Users.ToList();
        if (users == null)
        {
            return NotFound();
        }
        return View(users);
    }

    public async Task<IActionResult> SendMessage(string? message)
    {
        int currentUserId = int.Parse(_userManager.GetUserId(User));
        Message NewMessage = null;
        if (!string.IsNullOrEmpty(message))
        {
             NewMessage = new Message()
            {
                Content = message,
                Date = DateTime.UtcNow,
                UserId = currentUserId
            };
            var result = await _context.Messages.AddAsync(NewMessage);
            if (result != null)
            {
                await _context.SaveChangesAsync();
            }
        }
        return Json(NewMessage);
    }

    public IActionResult GetMessages()
    {
        var messages = _context.Messages.Take(30).Include(m => m.User).OrderBy(m => m.Date).ToList();
        if (messages == null)
        {
            return NotFound();
        }
        return PartialView("_Messages",messages);
    }
}