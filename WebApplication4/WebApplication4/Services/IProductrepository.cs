using WebApplication4.Models;

namespace WebApplication4.Services;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken ct = default);
}