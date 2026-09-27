using ECommerce.Application.Features.Product;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Endpoints;

/// <summary>商品模块端点：/api/v1/products。</summary>
public static class ProductEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("products", "Product");

        group.MapGet("", GetPage);                 // GET   /api/v1/products        分页列表
        group.MapGet("meta/categories", GetCategories); // GET /api/v1/products/meta/categories 分类列表
        group.MapGet("{id:long}", GetById);        // GET   /api/v1/products/{id}   详情
        group.MapPost("", Create);                 // POST  /api/v1/products        新增
        group.MapPut("{id:long}", Update);         // PUT   /api/v1/products/{id}   编辑
        group.MapDelete("{id:long}", Delete);      // DELETE /api/v1/products/{id}  删除
    }

    /// <summary>分页列表</summary>
    private static async Task<ApiResult> GetPage(
        [FromServices] ProductService service,
        [FromQuery] string? keyword = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] short? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var data = await service.GetPageAsync(keyword, categoryId, status, page, pageSize, ct);
        return ApiResult.Ok(data);
    }

    /// <summary>详情</summary>
    private static async Task<ApiResult> GetById(long id,
        [FromServices] ProductService service, CancellationToken ct)
    {
        var data = await service.GetByIdAsync(id, ct);
        return ApiResult.Ok(data);
    }

    /// <summary>新增</summary>
    private static async Task<ApiResult> Create([FromBody] ProductFormDto dto,
        [FromServices] ProductService service, CancellationToken ct)
    {
        await service.CreateAsync(dto, ct);
        return ApiResult.Ok(null, "新增成功");
    }

    /// <summary>编辑</summary>
    private static async Task<ApiResult> Update(long id, [FromBody] ProductFormDto dto,
        [FromServices] ProductService service, CancellationToken ct)
    {
        await service.UpdateAsync(id, dto, ct);
        return ApiResult.Ok(null, "修改成功");
    }

    /// <summary>删除</summary>
    private static async Task<ApiResult> Delete(long id,
        [FromServices] ProductService service, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return ApiResult.Ok(null, "删除成功");
    }

    /// <summary>分类列表</summary>
    private static async Task<ApiResult> GetCategories(
        [FromServices] ProductService service, CancellationToken ct)
    {
        var data = await service.GetCategoriesAsync(ct);
        return ApiResult.Ok(data);
    }
}
