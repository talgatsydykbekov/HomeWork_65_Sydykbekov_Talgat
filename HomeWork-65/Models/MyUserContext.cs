using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeWork_65.Models;

public class MyUserContext : IdentityDbContext<MyUser, IdentityRole<int>, int>
{
    public MyUserContext(DbContextOptions<MyUserContext> options) : base(options) { }

    public DbSet<Message> Messages { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<MyUser>()
            .Property(u => u.Avatar)
            .HasDefaultValue("/images/avatars/default.png");
        
        
        
        base.OnModelCreating(builder);
    }
}