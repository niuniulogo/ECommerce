using FreeSql.DataAnnotations;


namespace ECommerce.Domain.Entities.Product;

/// <summary>
/// 商品主表
/// </summary>
[Table(Name = "products")]
public class Products
{
    /// <summary>
    /// 主键 ID
    /// </summary>
    [Column(Name = "id", IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    [Column(Name = "name", StringLength = 200, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品编码
    /// </summary>
    [Column(Name = "sku", StringLength = 64, IsNullable = false)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 分类 ID
    /// </summary>
    [Column(Name = "category_id", IsNullable = true)]
    public int? CategoryId { get; set; }

    /// <summary>
    /// 销售价
    /// </summary>
    [Column(Name = "price", DbType = "numeric(12,2)", IsNullable = false)]
    public decimal Price { get; set; } = 0m;

    /// <summary>
    /// 成本价
    /// </summary>
    [Column(Name = "cost", DbType = "numeric(12,2)", IsNullable = false)]
    public decimal Cost { get; set; } = 0m;

    /// <summary>
    /// 库存数量
    /// </summary>
    [Column(Name = "stock", IsNullable = false)]
    public int Stock { get; set; } = 0;

    /// <summary>
    /// 单位
    /// </summary>
    [Column(Name = "unit", StringLength = 20, IsNullable = false)]
    public string Unit { get; set; } = "件";

    /// <summary>
    /// 主图
    /// </summary>
    [Column(Name = "cover_url", DbType = "text", IsNullable = true)]
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 多图（JSONB）
    /// </summary>
    [Column(Name = "images", DbType = "jsonb", IsNullable = false)]
    public string Images { get; set; } = "[]";

    /// <summary>
    /// 商品描述
    /// </summary>
    [Column(Name = "description", DbType = "text", IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 状态：1 上架 / 0 下架
    /// </summary>
    [Column(Name = "status", IsNullable = false)]
    public short Status { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column(Name = "created_at", DbType = "timestamptz",
            IsNullable = false, ServerTime = DateTimeKind.Utc, CanUpdate = false)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column(Name = "updated_at", DbType = "timestamptz",
            IsNullable = false, ServerTime = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 软删除时间（非空表示已删除）
    /// </summary>
    [Column(Name = "deleted_at", DbType = "timestamptz", IsNullable = true)]
    public DateTime? DeletedAt { get; set; }
}