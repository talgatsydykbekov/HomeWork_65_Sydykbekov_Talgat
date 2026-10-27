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
        var messages = LatestMessages();
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

    [HttpPost]
    public async Task<IActionResult> SendMessage(string? message)
    {
        if (!int.TryParse(_userManager.GetUserId(User), out int currentUserId))
        {
            return Unauthorized();
        }
        message = message?.Trim();
        if (string.IsNullOrEmpty(message) || message.Length > 120)
        {
            return BadRequest("Сообщение должно содержать от 1 до 120 символов.");
        }
        var newMessage = new Message
        {
            Content = message,
            Date = DateTime.UtcNow,
            UserId = currentUserId
        };
        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();
        return Ok(new { id = newMessage.Id });
    }

    public IActionResult GetMessages()
    {
        var messages = LatestMessages();
        if (messages == null)
        {
            return NotFound();
        }
        return PartialView("_Messages",messages);
    }

    private List<Message> LatestMessages()
    {
        return _context.Messages.AsNoTracking()
            .Include(m => m.User)
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Id)
            .Take(30)
            .ToList()
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .ToList();
    }
}
