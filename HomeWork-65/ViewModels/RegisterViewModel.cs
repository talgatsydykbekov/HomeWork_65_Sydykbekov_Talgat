using System.ComponentModel.DataAnnotations;

namespace HomeWork_65.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [Display(Name = "Имя пользователя")]
    public string UserName { get; set; }
    
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    [Required(ErrorMessage = "Email обязателен.")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [Display(Name = "Дата рождения")]
    [Range(18,100,ErrorMessage = "Возраст должен быть выше 18 лет")]
    public int Age { get; set; } 
    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Пароль должен содержать хотя бы 1 заглавную букву, 1 строчную букву и 1 цифру.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Подтвердить пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтвердить пароль")]
    public string PasswordConfirm { get; set; }
    
    [Required(ErrorMessage = "Загрузите аватар")]
    public IFormFile Avatar { get; set; }
}