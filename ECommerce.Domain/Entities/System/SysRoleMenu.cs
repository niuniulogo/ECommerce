using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_role_menu")]
public class SysRoleMenu
{
    [Column(Name = "role_id", IsPrimary = true)]
    public long RoleId { get; set; }

    [Column(Name = "menu_id", IsPrimary = true)]
    public long MenuId { get; set; }
}