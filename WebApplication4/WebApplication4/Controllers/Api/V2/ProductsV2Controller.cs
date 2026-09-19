using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication4.Infrastructure;
using WebApplication4.Models;
using WebApplication4.Services;
using WebApplication4.ViewModels;

namespace WebApplication4.Controllers.Api.V2;

/// <summary>
/// API v2: те же REST-возможности, что в v1 (ETag/304, If-Match/412,
/// Idempotency-Key, ProblemDetails), но DTO с 2 дополнительными полями.
/// Расписание версий:
///   v1: /api/products    → ProductDto   (Id, Name, Price, Category)
///   v2: /api/v2/products → ProductDtoV2 (+ Description, Stock)
/// </summary>
[ApiController]
[Route("api/v2/products")]
public class ProductsV2Controller : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly IdempotencyService _idem;
    private static readonly JsonSerializerOptions JsonWeb = new(JsonSerializerDefaults.Web);

    public ProductsV2Controller(IProductRepository repo, IdempotencyService idem)
    {
        _repo = repo;
        _idem = idem;
    }

    // GET /api/v2/products?category=Мебель
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
        return Ok(items.Select(ToDtoV2));
    }

    // GET /api/v2/products/3
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
        return Ok(ToDtoV2(product));
    }

    // POST /api/v2/products — 201 + Location + Idempotency-Key
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

        var dto = ToDtoV2(created);
        var location = $"/api/v2/products/{created.Id}";

        if (!string.IsNullOrWhiteSpace(idemKey))
            _idem.Save(idemKey, StatusCodes.Status201Created,
                JsonSerializer.Serialize(dto, JsonWeb), location);

        return Created(location, dto);
    }

    // PUT /api/v2/products/3 — полная замена, If-Match → 412
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
        return Ok(ToDtoV2(updated));
    }

    // PATCH /api/v2/products/3 — частично + Idempotency-Key
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
        var dto = ToDtoV2(updated);
        Response.Headers.ETag = EntityTags.For(updated);

        if (!string.IsNullOrWhiteSpace(idemKey))
            _idem.Save(idemKey, StatusCodes.Status200OK,
                JsonSerializer.Serialize(dto, JsonWeb), $"/api/v2/products/{updated.Id}");

        return Ok(dto);
    }

    // DELETE /api/v2/products/3 — 204, повтор → 404
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var deleted = await _repo.DeleteAsync(id, ct);
        if (!deleted)
            return NotFoundProblem(id);

        return NoContent();
    }

    // --- helpers ---

    private static ProductDtoV2 ToDtoV2(Product p)
        => new(p.Id, p.Name, p.Price, p.Category, p.Description, p.Stock);

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
