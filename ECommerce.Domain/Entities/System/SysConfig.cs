using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_config")]
public class SysConfig
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    [Column(Name = "config_name")] public string ConfigName { get; set; } = string.Empty;
    [Column(Name = "config_key")] public string ConfigKey { get; set; } = string.Empty;
    [Column(Name = "config_value")] public string ConfigValue { get; set; } = string.Empty;
    public string? Remark { get; set; }
    [Column(Name = "create_by")] public long? CreateBy { get; set; }
    [Column(Name = "update_by")] public long? UpdateBy { get; set; }
    [Column(Name = "is_deleted")] public int IsDeleted { get; set; }
    [Column(Name = "create_time")] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    [Column(Name = "update_time")] public DateTime? UpdateTime { get; set; }
}