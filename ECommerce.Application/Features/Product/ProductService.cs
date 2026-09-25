using ECommerce.Domain.Entities.Product;
using ECommerce.Domain.Repositories.Product;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Results;
using System.Text.Json.Serialization;

namespace ECommerce.Application.Features.Product;

public class ProductService
{
    private readonly IProductRepository _products;

    public ProductService(IProductRepository products)
    {
        _products = products;
    }

    public async Task<object> GetPageAsync(string? keyword, int? categoryId, short? status, int pageNum, int pageSize,
        CancellationToken ct)
    {
        var (items, total) = await _products.QueryPageAsync(keyword, categoryId, status, pageNum, pageSize, ct);
        var categories = await _products.GetCategoriesAsync(ct);
        var catDict = categories.ToDictionary(c => c.Id, c => c.Name);

        var list = items.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            sku = p.Sku,
            category_id = p.CategoryId,
            category_name = p.CategoryId.HasValue && catDict.ContainsKey(p.CategoryId.Value)
                ? catDict[p.CategoryId.Value]
                : null,
            price = p.Price,
            cost = p.Cost,
            stock = p.Stock,
            unit = p.Unit,
            cover_url = p.CoverUrl,
            description = p.Description,
            status = p.Status,
            created_at = p.CreatedAt,
            updated_at = p.UpdatedAt
        });

        return new { list, total };
    }

    public async Task<object> GetByIdAsync(long id, CancellationToken ct)
    {
        var p = await _products.GetByIdAsync(id, ct)
            ?? throw new BusinessException("商品不存在", ResultCodes.NotFound);

        return new
        {
            id = p.Id,
            name = p.Name,
            sku = p.Sku,
            category_id = p.CategoryId,
            price = p.Price,
            cost = p.Cost,
            stock = p.Stock,
            unit = p.Unit,
            cover_url = p.CoverUrl,
            description = p.Description,
            status = p.Status
        };
    }

    public async Task CreateAsync(ProductFormDto dto, CancellationToken ct)
    {
        if (await _products.ExistsSkuAsync(dto.Sku, null, ct))
            throw new BusinessException("SKU 已存在", ResultCodes.Conflict);

        var product = new Products
        {
            Name = dto.Name,
            Sku = dto.Sku,
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            Cost = dto.Cost,
            Stock = dto.Stock,
            Unit = dto.Unit ?? "件",
            CoverUrl = dto.CoverUrl,
            Description = dto.Description,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _products.InsertAsync(product, ct);
    }

    public async Task UpdateAsync(long id, ProductFormDto dto, CancellationToken ct)
    {
        var p = await _products.GetByIdAsync(id, ct)
            ?? throw new BusinessException("商品不存在", ResultCodes.NotFound);

        if (await _products.ExistsSkuAsync(dto.Sku, id, ct))
            throw new BusinessException("SKU 已存在", ResultCodes.Conflict);

        p.Name = dto.Name;
        p.Sku = dto.Sku;
        p.CategoryId = dto.CategoryId;
        p.Price = dto.Price;
        p.Cost = dto.Cost;
        p.Stock = dto.Stock;
        p.Unit = dto.Unit ?? p.Unit;
        p.CoverUrl = dto.CoverUrl;
        p.Description = dto.Description;
        p.Status = dto.Status;
        p.UpdatedAt = DateTime.UtcNow;

        await _products.UpdateAsync(p, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        await _products.DeleteAsync(id, ct);
    }

    public async Task<List<object>> GetCategoriesAsync(CancellationToken ct)
    {
        var cats = await _products.GetCategoriesAsync(ct);
        return cats.Select(c => new { id = c.Id, name = c.Name, sort_order = c.SortOrder }).Cast<object>().ToList();
    }
}

public class ProductFormDto
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    [JsonPropertyName("category_id")]
    public int? CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int Stock { get; set; }
    public string? Unit { get; set; }
    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; set; }
    public string? Description { get; set; }
    public short Status { get; set; } = 1;
}
