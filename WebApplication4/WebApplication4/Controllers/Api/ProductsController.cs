using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new[] {
            new { Id = 1, Name = "Товар A", Price = 100 },
            new { Id = 2, Name = "Товар B", Price = 200 }
        });
}