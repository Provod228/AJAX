using System.ComponentModel.DataAnnotations;

namespace WebApplication4.ViewModels;

/// <summary>
/// Request DTO для POST (создание). Входные данные — отдельным классом,
/// с валидацией из конспекта. Ошибки → 400 ValidationProblem (автоматом от [ApiController]).
/// </summary>
public class CreateProductRequest
{
    [Required(ErrorMessage = "Имя обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя — от 2 до 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1_000_000, ErrorMessage = "Цена — от 0.01 до 1000000")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Категория обязательна")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Категория — от 2 до 50 символов")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Описание — максимум 500 символов")]
    public string? Description { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Остаток — от 0 до 1000000")]
    public int Stock { get; set; }
}

/// <summary>
/// Request DTO для PUT (полная замена): все поля обязательны, кроме описания.
/// </summary>
public class UpdateProductRequest
{
    [Required(ErrorMessage = "Имя обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя — от 2 до 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1_000_000, ErrorMessage = "Цена — от 0.01 до 1000000")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Категория обязательна")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Категория — от 2 до 50 символов")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Описание — максимум 500 символов")]
    public string? Description { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Остаток — от 0 до 1000000")]
    public int Stock { get; set; }
}

/// <summary>
/// Request DTO для PATCH (частичное изменение): null = поле не прислано, не трогаем.
/// Упрощённый вариант Optional&lt;T&gt; из конспекта.
/// </summary>
public class PatchProductRequest
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя — от 2 до 100 символов")]
    public string? Name { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Цена — от 0.01 до 1000000")]
    public decimal? Price { get; set; }

    [StringLength(50, MinimumLength = 2, ErrorMessage = "Категория — от 2 до 50 символов")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Описание — максимум 500 символов")]
    public string? Description { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Остаток — от 0 до 1000000")]
    public int? Stock { get; set; }
}
