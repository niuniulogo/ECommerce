using ECommerce.Application.Features.Product;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>分页列表</summary>
    [HttpGet]
    public async Task<ApiResult> GetList([FromQuery] string? keyword, [FromQuery] int? categoryId,
        [FromQuery] short? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var data = await _productService.GetPageAsync(keyword, categoryId, status, page, pageSize, ct);
        return ApiResult.Ok(data);
    }

    /// <summary>详情</summary>
    [HttpGet("{id:long}")]
    public async Task<ApiResult> GetDetail(long id, CancellationToken ct)
    {
        var data = await _productService.GetByIdAsync(id, ct);
        return ApiResult.Ok(data);
    }

    /// <summary>新增</summary>
    [HttpPost]
    public async Task<ApiResult> Create([FromBody] ProductFormDto dto, CancellationToken ct)
    {
        await _productService.CreateAsync(dto, ct);
        return ApiResult.Ok(null, "新增成功");
    }

    /// <summary>编辑</summary>
    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] ProductFormDto dto, CancellationToken ct)
    {
        await _productService.UpdateAsync(id, dto, ct);
        return ApiResult.Ok(null, "修改成功");
    }

    /// <summary>删除</summary>
    [HttpDelete("{id:long}")]
    public async Task<ApiResult> Delete(long id, CancellationToken ct)
    {
        await _productService.DeleteAsync(id, ct);
        return ApiResult.Ok(null, "删除成功");
    }

    /// <summary>分类列表</summary>
    [HttpGet("meta/categories")]
    public async Task<ApiResult> GetCategories(CancellationToken ct)
    {
        var data = await _productService.GetCategoriesAsync(ct);
        return ApiResult.Ok(data);
    }
}
