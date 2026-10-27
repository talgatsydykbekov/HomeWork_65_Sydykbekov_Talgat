using HomeWork_65.Models;
using HomeWork_65.ViewModels;
using HomeWork_65.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace HomeWork_65.Controllers;

public class AccountController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<MyUser> _userManager;
    private readonly SignInManager<MyUser> _signInManager;
    private readonly EmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<MyUser> userManager, SignInManager<MyUser> signInManager,
        IWebHostEnvironment env, EmailService emailService, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
        _emailService = emailService;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (_userManager.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Пользователь с таким Email уже существует.");
                return View(model);
            }
            if (_userManager.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Пользователь с таким именем уже существует.");
                return View(model);
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
                    await _signInManager.SignInAsync(user, false);
                    try
                    {
                        string profileUrl = Url.Action("Details", "MyUser",
                            new { id = user.Id }, Request.Scheme)
                            ?? throw new InvalidOperationException("Не удалось создать ссылку на профиль.");

                        string login = HtmlEncoder.Default.Encode(user.UserName!);
                        string link = HtmlEncoder.Default.Encode(profileUrl);

                        await _emailService.SendEmailAsync(
                            user.Email!,
                            "Добро пожаловать в Ajax Chat!",
                            $"<h2>Добро пожаловать, {login}!</h2>" +
                            $"<p>Ваш логин: <strong>{login}</strong></p>" +
                            $"<p><a href=\"{link}\">Открыть мой профиль</a></p>");

                        TempData["RegistrationNotice"] =
                            "Регистрация завершена. Приветственное письмо отправлено на вашу почту.";
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception,
                            "Не удалось отправить приветственное письмо пользователю {UserId}.", user.Id);
                        TempData["RegistrationNotice"] =
                            "Аккаунт создан, но отправить приветственное письмо не удалось. Вы можете пользоваться чатом.";
                    }
                    return RedirectToAction("Index", "Chat");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        return View(new LoginViewModel {ReturnUrl = returnUrl});
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            MyUser user = await _userManager.FindByEmailAsync(model.EmailOrLogin);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.EmailOrLogin);
            }
            if (user == null)
            {
                ModelState.AddModelError("","Пользователь не найден");
                return View(model);
            }
            
            SignInResult result = await _signInManager.PasswordSignInAsync(
                user, model.Password, 
                model.RememberMe,
                false
                );
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Chat");
            }
            ModelState.AddModelError("", "Неправильный логин или пароль");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOff()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
