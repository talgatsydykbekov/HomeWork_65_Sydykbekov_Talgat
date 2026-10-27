using HomeWork_65.Models;
using HomeWork_65.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<MyUserContext>(options => options.UseNpgsql(connectionString)).
            AddIdentity<MyUser, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                
            }). 
            AddEntityFrameworkStores<MyUserContext>();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
        });
        builder.Services.AddTransient<EmailService>();
        var app = builder.Build();
        if (app.Environment.IsDevelopment()
            && app.Configuration.GetValue<bool>("TestEmail"))
        {
            using var emailScope = app.Services.CreateScope();

            var emailService = emailScope.ServiceProvider
                .GetRequiredService<EmailService>();

            string recipient = app.Configuration["EmailSettings:Email"]
                               ?? throw new InvalidOperationException(
                                   "Не указан email для тестового письма.");

            await emailService.SendEmailAsync(
                recipient,
                "Проверка отправки из Ajax Chat",
                "<h2>Почта работает!</h2><p>Тестовое письмо из проекта.</p>");

            Console.WriteLine("Тестовое письмо отправлено.");
            return;
        }
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var userManager = services.GetRequiredService<UserManager<MyUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                await AdminInitializer.SeedAdminUser(roleManager, userManager);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        
        app.Run();
    }
}

