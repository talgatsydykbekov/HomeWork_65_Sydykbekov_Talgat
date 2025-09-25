using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HomeWork_65.Models;

public class MyUser : IdentityUser<int>
{
    [Required(ErrorMessage = "Выберите файл для профиля")]
    public string Avatar { get; set; } 
    [Range(18,100,ErrorMessage = "Возраст должен быть выше 18 лет")]
    public int Age { get; set; }    
    
    public bool IsBlocked { get; set; } = false;
}