using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.Product;

/// <summary>
/// 商品分类表
/// </summary>
[Table(Name = "categories")]
public class Categories
{
    /// <summary>
    /// 主键 ID
    /// </summary>
    [Column(Name = "id", IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    /// <summary>
    /// 分类名称
    /// </summary>
    [Column(Name = "name", StringLength = 50, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排序序号
    /// </summary>
    [Column(Name = "sort_order", IsNullable = false)]
    public int SortOrder { get; set; } = 0;

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
}