using System.ComponentModel.DataAnnotations;

namespace HomeWork_65.ViewModels;

public class EditProfileViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите имя пользователя.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите email.")]
    [EmailAddress(ErrorMessage = "Некорректный email.")]
    public string Email { get; set; } = string.Empty;

    [Range(18, 100, ErrorMessage = "Возраст должен быть от 18 до 100 лет.")]
    public int Age { get; set; }

    public IFormFile? Avatar { get; set; }
}
