using ECommerce.Domain.Entities.Product;

namespace ECommerce.Domain.Repositories.Product;

public interface IProductRepository
{
    Task<(List<Products> Items, long Total)> QueryPageAsync(string? keyword, int? categoryId, short? status, int pageNum, int pageSize, CancellationToken ct = default);
    Task<Products?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<long> InsertAsync(Products product, CancellationToken ct = default);
    Task UpdateAsync(Products product, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<Categories>> GetCategoriesAsync(CancellationToken ct = default);
    Task<bool> ExistsSkuAsync(string sku, long? excludeId, CancellationToken ct = default);
}
