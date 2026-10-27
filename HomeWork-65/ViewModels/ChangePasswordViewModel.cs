using System.ComponentModel.DataAnnotations;

namespace HomeWork_65.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Введите текущий пароль.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите новый пароль.")]
    [MinLength(6, ErrorMessage = "Минимум 6 символов.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите новый пароль.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
