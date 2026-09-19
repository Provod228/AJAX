using System.ComponentModel.DataAnnotations;

namespace WebApplication4.ViewModels;

/// <summary>
/// ViewModel формы добавления товара (Home/Create).
/// Валидация — DataAnnotations, ошибки показываются в самой форме.
/// </summary>
public class ProductCreateViewModel
{
    [Required(ErrorMessage = "Введите название товара")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название — от 2 до 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите цену")]
    [Range(0.01, 10_000_000, ErrorMessage = "Цена должна быть больше 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Введите категорию")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Категория — от 2 до 50 символов")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Описание — максимум 500 символов")]
    public string? Description { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Остаток — от 0 до 1 000 000")]
    public int Stock { get; set; }

    /// <summary>Существующие категории — для подсказок (datalist) в форме.</summary>
    public IReadOnlyList<string> ExistingCategories { get; set; } = Array.Empty<string>();
}
