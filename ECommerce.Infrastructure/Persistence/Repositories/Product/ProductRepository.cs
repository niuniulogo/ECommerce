using ECommerce.Domain.Entities.Product;
using ECommerce.Domain.Repositories.Product;
using FreeSql;

namespace ECommerce.Infrastructure.Persistence.Repositories.Product;

public class ProductRepository : IProductRepository
{
    private readonly IFreeSql _fsql;

    public ProductRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public async Task<(List<Products> Items, long Total)> QueryPageAsync(string? keyword, int? categoryId, short? status,
        int pageNum, int pageSize, CancellationToken ct = default)
    {
        var select = _fsql.Select<Products>()
            .Where(x => x.DeletedAt == null)
            .WhereIf(!string.IsNullOrWhiteSpace(keyword),
                x => x.Name.Contains(keyword!) || x.Sku.Contains(keyword!))
            .WhereIf(categoryId.HasValue, x => x.CategoryId == categoryId)
            .WhereIf(status.HasValue, x => x.Status == status);

        var total = await select.CountAsync(ct);
        var items = await select
            .OrderByDescending(x => x.Id)
            .Page(pageNum, pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Products?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<Products>()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .FirstAsync(ct)!;
    }

    public async Task<long> InsertAsync(Products product, CancellationToken ct = default)
    {
        return await _fsql.Insert(product).ExecuteIdentityAsync(ct);
    }

    public async Task UpdateAsync(Products product, CancellationToken ct = default)
    {
        await _fsql.Update<Products>()
            .SetSource(product)
            .IgnoreColumns(x => new { x.CreatedAt })
            .ExecuteAffrowsAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        // 软删除：设置 DeletedAt
        await _fsql.Update<Products>()
            .Set(x => x.DeletedAt, DateTime.UtcNow)
            .Where(x => x.Id == id)
            .ExecuteAffrowsAsync(ct);
    }

    public Task<List<Categories>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return _fsql.Select<Categories>()
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);
    }

    public Task<bool> ExistsSkuAsync(string sku, long? excludeId, CancellationToken ct = default)
    {
        return _fsql.Select<Products>()
            .Where(x => x.Sku == sku && x.DeletedAt == null)
            .WhereIf(excludeId.HasValue, x => x.Id != excludeId)
            .AnyAsync(ct);
    }
}
