using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_role_dept")]
public class SysRoleDept
{
    [Column(Name = "role_id", IsPrimary = true)]
    public long RoleId { get; set; }

    [Column(Name = "dept_id", IsPrimary = true)]
    public long DeptId { get; set; }
}