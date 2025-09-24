using System.ComponentModel.DataAnnotations;

namespace HomeWork_65.Models;

public class MyUser
{
    [Required(ErrorMessage = "Выберите файл для профиля")]
    public string Avatar { get; set; } 
    
    
    
}