using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_dict")]
public class SysDict
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    [Column(Name = "dict_code")] public string? DictCode { get; set; }
    public string? Name { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    [Column(Name = "create_by")] public long? CreateBy { get; set; }
    [Column(Name = "update_by")] public long? UpdateBy { get; set; }
    [Column(Name = "is_deleted")] public int IsDeleted { get; set; }
    [Column(Name = "create_time")] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    [Column(Name = "update_time")] public DateTime? UpdateTime { get; set; }
}