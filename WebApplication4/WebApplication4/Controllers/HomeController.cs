using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using WebApplication4.Services;
using WebApplication4.ViewModels;

namespace WebApplication4.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _repo;

    public HomeController(IProductRepository repo) => _repo = repo;

    // UI-экшен: возвращает View (shell)
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Берём только категории для фильтров — не весь список товаров
        var all = await _repo.GetAllAsync(ct);
        var categories = all.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

        var vm = new ProductListViewModel
        {
            Title = "Каталог товаров",
            ApiEndpoint = Url.Action("List", "Products")!,  // /Products/List
            Categories = categories
        };

        return View(vm);   // Razor получает только метаданные
    }

    public IActionResult Privacy() => View();

    // Вторая страница: информация только об ОДНОМ товаре (/Home/Details/3)
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFound();

        return View(product);
    }

    // Форма добавления товара: показать пустую форму
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var all = await _repo.GetAllAsync(ct);
        return View(new ProductCreateViewModel
        {
            ExistingCategories = all.Select(p => p.Category).Distinct().OrderBy(c => c).ToList()
        });
    }

    // Форма добавления товара: принять и сохранить, затем редирект на страницу товара
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var all = await _repo.GetAllAsync(ct);
            vm.ExistingCategories = all.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(vm);
        }

        var created = await _repo.AddAsync(new Product
        {
            Name = vm.Name.Trim(),
            Price = vm.Price,
            Category = vm.Category.Trim(),
            Description = vm.Description?.Trim() ?? string.Empty,
            Stock = vm.Stock
        }, ct);

        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    public IActionResult Error() => View();
}