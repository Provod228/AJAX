using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using WebApplication4.Services;
using WebApplication4.ViewModels;

namespace WebApplication4.Controllers.Api.V2;

/// <summary>
/// API v2: версия каталога с 2 дополнительными полями (Description, Stock).
/// V1 (api/products, ProductDto из 4 полей) не тронута и работает как раньше.
///
/// Расписание версий:
///   v1: GET /api/products,        GET /api/products/{id}        -> ProductDto   (Id, Name, Price, Category)
///   v2: GET /api/v2/products,     GET /api/v2/products/{id}     -> ProductDtoV2 (те же + Description, Stock)
/// </summary>
[Route("api/v2/products")]
public class ProductsV2Controller : Controller
{
    private readonly IProductRepository _repo;

    public ProductsV2Controller(IProductRepository repo) => _repo = repo;

    // GET /api/v2/products?category=Мебель
    [HttpGet("")]
    public async Task<IActionResult> List([FromQuery] string? category, CancellationToken ct)
    {
        var items = string.IsNullOrWhiteSpace(category)
            ? await _repo.GetAllAsync(ct)
            : await _repo.GetByCategoryAsync(category, ct);

        return Json(items.Select(ToDtoV2));
    }

    // GET /api/v2/products/3
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFound(new { error = "Product not found", id });

        return Json(ToDtoV2(product));
    }

    private static ProductDtoV2 ToDtoV2(Product p)
        => new(p.Id, p.Name, p.Price, p.Category, p.Description, p.Stock);
}
