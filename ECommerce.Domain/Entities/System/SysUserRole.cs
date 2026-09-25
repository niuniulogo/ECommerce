using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_user_role")]
public class SysUserRole
{
    [Column(Name = "user_id", IsPrimary = true)]
    public long UserId { get; set; }

    [Column(Name = "role_id", IsPrimary = true)]
    public long RoleId { get; set; }
}