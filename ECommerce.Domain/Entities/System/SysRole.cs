using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_role")]
public class SysRole
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
    [Column(Name = "data_scope")] public int? DataScope { get; set; }
    [Column(Name = "create_by")] public long? CreateBy { get; set; }
    [Column(Name = "update_by")] public long? UpdateBy { get; set; }
    [Column(Name = "is_deleted")] public int IsDeleted { get; set; }
    [Column(Name = "create_time")] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    [Column(Name = "update_time")] public DateTime? UpdateTime { get; set; }
}