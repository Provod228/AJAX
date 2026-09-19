namespace WebApplication4.ViewModels;

/// <summary>
/// DTO версии v2: все поля v1 + 2 новых (Description, Stock).
/// V1 (ProductDto) намеренно не меняется — обратная совместимость.
/// </summary>
public record ProductDtoV2(
    int Id,
    string Name,
    decimal Price,
    string Category,
    string Description,
    int Stock);
