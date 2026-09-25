using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_user")]
public class SysUser
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public int Gender { get; set; } = 1;
    public string Password { get; set; } = string.Empty;
    [Column(Name = "dept_id")] public long? DeptId { get; set; }
    public string? Avatar { get; set; }
    public string? Mobile { get; set; }
    public int Status { get; set; } = 1;
    public string? Email { get; set; }
    [Column(Name = "create_by")] public long? CreateBy { get; set; }
    [Column(Name = "update_by")] public long? UpdateBy { get; set; }
    [Column(Name = "is_deleted")] public int IsDeleted { get; set; }
    [Column(Name = "create_time")] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    [Column(Name = "update_time")] public DateTime? UpdateTime { get; set; }
}