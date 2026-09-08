using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "Нужно обязательно заполнить имя ")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя от 2 до 50")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Нужно обязательно заполнить eamil")]
        [EmailAddress(ErrorMessage = "Нужна правильна почта")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Нужно обязательно заполнить год рождения")]
        [Range(1900, 2026, ErrorMessage = "Год от 1900 до 2026 символов")]
        public int BirthYear { get; set;  }

        [Required(ErrorMessage = "Нужно обязательно заполнить пароль")]
        [StringLength(6, ErrorMessage = "Пароль от 6 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set;} = string.Empty;
    }

    public class AgeResultModel
    {
        public string UserName { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsAdualt => Age >= 18;
    }
}
