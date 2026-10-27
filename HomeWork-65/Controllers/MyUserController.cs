using HomeWork_65.Models;
using HomeWork_65.ViewModels;
using HomeWork_65.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeWork_65.Controllers;

[Authorize]
public class MyUserController : Controller
{
    private MyUserContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<MyUser> _userManager;
    private readonly SignInManager<MyUser> _signInManager;
    private readonly EmailService _emailService;
    private readonly ILogger<MyUserController> _logger;

    public MyUserController(UserManager<MyUser> userManager, SignInManager<MyUser> signInManager,
        MyUserContext context, IWebHostEnvironment env, EmailService emailService,
        ILogger<MyUserController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _env = env;
        _emailService = emailService;
        _logger = logger;
    }
    public IActionResult Details(int? id)
    {
        if (id.HasValue)
        {
            var user = _context.Users.Include(u => u.Messages.Where(u => u.UserId == id)).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        return NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue) return NotFound();
        if (!CanEdit(id.Value)) return Forbid();
        var user = await _userManager.FindByIdAsync(id.Value.ToString());
        if (user == null) return NotFound();

        return View(new EditProfileViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Age = user.Age
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!CanEdit(model.Id)) return Forbid();
        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        if (user == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        string normalizedEmail = _userManager.NormalizeEmail(model.Email);
        if (await _userManager.Users.AnyAsync(u => u.Id != user.Id && u.NormalizedEmail == normalizedEmail))
            ModelState.AddModelError(nameof(model.Email), "Этот email уже используется.");

        string normalizedName = _userManager.NormalizeName(model.UserName);
        if (await _userManager.Users.AnyAsync(u => u.Id != user.Id && u.NormalizedUserName == normalizedName))
            ModelState.AddModelError(nameof(model.UserName), "Этот логин уже используется.");

        string? extension = null;
        if (model.Avatar != null)
        {
            extension = Path.GetExtension(model.Avatar.FileName).ToLowerInvariant();
            if (model.Avatar.Length == 0 || model.Avatar.Length > 5 * 1024 * 1024 ||
                !new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension))
                ModelState.AddModelError(nameof(model.Avatar), "Выберите изображение JPG, PNG, GIF или WebP до 5 МБ.");
        }
        if (!ModelState.IsValid) return View(model);

        string? oldEmail = user.Email;
        var changes = new List<string>();
        if (user.UserName != model.UserName)
            changes.Add($"Логин: {HtmlEncoder.Default.Encode(model.UserName)}");
        if (user.Email != model.Email)
            changes.Add($"Email: {HtmlEncoder.Default.Encode(model.Email)}");
        if (user.Age != model.Age)
            changes.Add($"Возраст: {model.Age}");

        if (model.Avatar != null)
        {
            string directory = Path.Combine(_env.WebRootPath, "images", "avatars");
            Directory.CreateDirectory(directory);
            string fileName = $"{Guid.NewGuid()}{extension}";
            await using (var stream = System.IO.File.Create(Path.Combine(directory, fileName)))
                await model.Avatar.CopyToAsync(stream);
            user.Avatar = $"images/avatars/{fileName}";
            changes.Add("Аватар обновлён.");
        }

        if (changes.Count == 0)
        {
            TempData["ProfileNotice"] = "Изменений нет.";
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        user.UserName = model.UserName;
        if (user.Email != model.Email) user.EmailConfirmed = false;
        user.Email = model.Email;
        user.Age = model.Age;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        if (_userManager.GetUserId(User) == user.Id.ToString())
            await _signInManager.RefreshSignInAsync(user);

        bool sent = await NotifyProfileChange(user, oldEmail, "Профиль изменён",
            "<h2>Данные вашего профиля изменены</h2><ul><li>" +
            string.Join("</li><li>", changes) + "</li></ul>");
        TempData["ProfileNotice"] = sent
            ? "Профиль сохранён. Уведомление отправлено на почту."
            : "Профиль сохранён, но отправить уведомление на почту не удалось.";
        return RedirectToAction(nameof(Details), new { id = user.Id });
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        bool sent = await NotifyProfileChange(user, user.Email, "Пароль изменён",
            "<p>Пароль вашего аккаунта Ajax Chat успешно изменён.</p>");
        TempData["ProfileNotice"] = sent
            ? "Пароль изменён. Уведомление отправлено на почту."
            : "Пароль изменён, но отправить уведомление на почту не удалось.";
        return RedirectToAction(nameof(Details), new { id = user.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMyData()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new { message = "Войдите в аккаунт заново." });
        if (string.IsNullOrWhiteSpace(user.Email))
            return BadRequest(new { message = "Сначала укажите email в профиле." });

        int messageCount = await _context.Messages.CountAsync(m => m.UserId == user.Id);
        try
        {
            string profileUrl = Url.Action(nameof(Details), "MyUser",
                new { id = user.Id }, Request.Scheme)
                ?? throw new InvalidOperationException("Не удалось создать ссылку на профиль.");

            string login = HtmlEncoder.Default.Encode(user.UserName ?? "");
            string link = HtmlEncoder.Default.Encode(profileUrl);
            await _emailService.SendEmailAsync(user.Email, "Ваши данные в Ajax Chat",
                "<h2>Данные вашего профиля</h2>" +
                $"<p>Логин: {login}</p>" +
                $"<p>Возраст: {user.Age}</p>" +
                $"<p>Отправлено сообщений: {messageCount}</p>" +
                $"<p><a href=\"{link}\">Открыть мой профиль</a></p>");

            return Ok(new { message = "Ваши данные отправлены на почту, указанную в профиле." });
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Не удалось отправить данные пользователю {UserId}.", user.Id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Не удалось отправить письмо. Попробуйте позже." });
        }
    }

    private bool CanEdit(int id) =>
        _userManager.GetUserId(User) == id.ToString() || User.IsInRole("admin");

    private async Task<bool> NotifyProfileChange(MyUser user, string? oldEmail, string subject, string body)
    {
        bool sent = true;
        var recipients = new[] { oldEmail, user.Email }
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (recipients.Count == 0) return false;
        foreach (var email in recipients)
        {
            try
            {
                await _emailService.SendEmailAsync(email!, subject, body);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Не удалось отправить уведомление пользователю {UserId}.", user.Id);
                sent = false;
            }
        }
        return sent;
    }
    
    [Authorize(Roles = "admin")]
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
    public async  Task<IActionResult> Create(RegisterViewModel model)
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
            
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Avatar.FileName)}";
            
                string filesavePath = Path.Combine(uploadedFileName, fileName);
                using (var stream = new FileStream(filesavePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(stream);
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
