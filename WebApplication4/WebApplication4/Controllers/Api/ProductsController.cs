using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication4.Infrastructure;
using WebApplication4.ModelBinders;
using WebApplication4.Models;
using WebApplication4.Services;
using WebApplication4.ViewModels;

namespace WebApplication4.Controllers;

/// <summary>
/// API v1 каталога — полный REST из конспекта:
/// GET (список/один, ETag → 304) · POST (201 + Location, Idempotency-Key) ·
/// PUT (полная замена, If-Match → 412) · PATCH (частично, Idempotency-Key) ·
/// DELETE (204, повторный → 404). Ошибки — ProblemDetails.
/// Биндинг по таблице конспекта: [FromRoute] / [FromQuery] / [FromBody] / [FromHeader].
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly IdempotencyService _idem;
    private static readonly JsonSerializerOptions JsonWeb = new(JsonSerializerDefaults.Web);

    public ProductsController(IProductRepository repo, IdempotencyService idem)
    {
        _repo = repo;
        _idem = idem;
    }

    // GET /api/products?category=Мебель — список (+ ETag → 304 Not Modified)
    [HttpGet("")]
    public async Task<IActionResult> List(
        [FromQuery] string? category,
        [FromHeader(Name = "If-None-Match")] string? noneMatch,
        CancellationToken ct)
    {
        var items = string.IsNullOrWhiteSpace(category)
            ? await _repo.GetAllAsync(ct)
            : await _repo.GetByCategoryAsync(category, ct);

        var etag = EntityTags.ForList(items);
        if (EntityTags.Matches(noneMatch, etag))
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = etag;
        return Ok(items.Select(ToDto));
    }

    // GET /api/products/3 — один (+ ETag → 304)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(
        [FromRoute] int id,
        [FromHeader(Name = "If-None-Match")] string? noneMatch,
        CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFoundProblem(id);

        var etag = EntityTags.For(product);
        if (EntityTags.Matches(noneMatch, etag))
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = etag;
        return Ok(ToDto(product));
    }

    // GET /api/products/by-ids?ids=1,2,3 — кастомный биндер (CsvIntArrayBinder).
    // ?ids=1,abc → 400 ValidationProblem (ошибка в ModelState).
    [HttpGet("by-ids")]
    public async Task<IActionResult> ByIds(
        [ModelBinder(typeof(CsvIntArrayBinder))] int[] ids,
        CancellationToken ct)
    {
        var items = await _repo.GetAllAsync(ct);
        return Ok(items.Where(p => ids.Contains(p.Id)).Select(ToDto));
    }

    // POST /api/products — создать (201 + Location). Idempotency-Key: повтор
    // с тем же ключом вернёт сохранённый ответ, дубликат не создастся.
    [HttpPost("")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest req,
        [FromHeader(Name = "Idempotency-Key")] string? idemKey,
        CancellationToken ct)
    {
        if (TryReplay(idemKey, out var replayed))
            return replayed;

        var created = await _repo.AddAsync(new Product
        {
            Name = req.Name.Trim(),
            Price = req.Price,
            Category = req.Category.Trim(),
            Description = req.Description?.Trim() ?? string.Empty,
            Stock = req.Stock
        }, ct);

        var dto = ToDto(created);
        var location = $"/api/products/{created.Id}";

        if (!string.IsNullOrWhiteSpace(idemKey))
            _idem.Save(idemKey, StatusCodes.Status201Created,
                JsonSerializer.Serialize(dto, JsonWeb), location);

        return Created(location, dto);
    }

    // PUT /api/products/3 — заменить полностью.
    // If-Match со старым ETag → 412 Precondition Failed.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Replace(
        [FromRoute] int id,
        [FromBody] UpdateProductRequest req,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFoundProblem(id);

        if (!string.IsNullOrWhiteSpace(ifMatch) && !EntityTags.Matches(ifMatch, EntityTags.For(product)))
            return PreconditionFailed($"Ресурс изменён: ожидался {ifMatch}, текущий {EntityTags.For(product)}");

        product.Name = req.Name.Trim();
        product.Price = req.Price;
        product.Category = req.Category.Trim();
        product.Description = req.Description?.Trim() ?? string.Empty;
        product.Stock = req.Stock;

        var updated = (await _repo.UpdateAsync(product, ct))!;
        Response.Headers.ETag = EntityTags.For(updated);
        return Ok(ToDto(updated));
    }

    // PATCH /api/products/3 — изменить частично (null = поле не прислано).
    // If-Match + Idempotency-Key поддерживаются, как в конспекте.
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(
        [FromRoute] int id,
        [FromBody] PatchProductRequest req,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        [FromHeader(Name = "Idempotency-Key")] string? idemKey,
        CancellationToken ct)
    {
        if (TryReplay(idemKey, out var replayed))
            return replayed;

        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null)
            return NotFoundProblem(id);

        if (!string.IsNullOrWhiteSpace(ifMatch) && !EntityTags.Matches(ifMatch, EntityTags.For(product)))
            return PreconditionFailed($"Ресурс изменён: ожидался {ifMatch}, текущий {EntityTags.For(product)}");

        if (req.Name is not null) product.Name = req.Name.Trim();
        if (req.Price.HasValue) product.Price = req.Price.Value;
        if (req.Category is not null) product.Category = req.Category.Trim();
        if (req.Description is not null) product.Description = req.Description;
        if (req.Stock.HasValue) product.Stock = req.Stock.Value;

        var updated = (await _repo.UpdateAsync(product, ct))!;
        var dto = ToDto(updated);
        Response.Headers.ETag = EntityTags.For(updated);

        if (!string.IsNullOrWhiteSpace(idemKey))
            _idem.Save(idemKey, StatusCodes.Status200OK,
                JsonSerializer.Serialize(dto, JsonWeb), $"/api/products/{updated.Id}");

        return Ok(dto);
    }

    // DELETE /api/products/3 — удалить: 204, повторный вызов → 404 (состояние то же).
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var deleted = await _repo.DeleteAsync(id, ct);
        if (!deleted)
            return NotFoundProblem(id);

        return NoContent();
    }

    // --- helpers ---

    private static ProductDto ToDto(Product p) => new(p.Id, p.Name, p.Price, p.Category);

    private IActionResult NotFoundProblem(int id) => Problem(
        title: "Product not found",
        detail: $"Product with id {id} does not exist",
        statusCode: StatusCodes.Status404NotFound,
        type: "https://example.com/errors/not-found");

    private IActionResult PreconditionFailed(string detail) => Problem(
        title: "Precondition Failed",
        detail: detail,
        statusCode: StatusCodes.Status412PreconditionFailed,
        type: "https://example.com/errors/precondition-failed");

    private bool TryReplay(string? key, out IActionResult replayed)
    {
        replayed = null!;
        if (string.IsNullOrWhiteSpace(key) || !_idem.TryGet(key, out var cached))
            return false;

        if (cached.Location is not null)
            Response.Headers.Location = cached.Location;
        Response.Headers["X-Idempotent-Replayed"] = "true";
        replayed = new ContentResult
        {
            StatusCode = cached.StatusCode,
            Content = cached.Body,
            ContentType = "application/json"
        };
        return true;
    }
}
