using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_menu")]
public class SysMenu
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    [Column(Name = "parent_id")] public long ParentId { get; set; }
    [Column(Name = "tree_path")] public string? TreePath { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "M";
    [Column(Name = "route_name")] public string? RouteName { get; set; }
    [Column(Name = "route_path")] public string? RoutePath { get; set; }
    public string? Component { get; set; }
    [Column(Name = "external_url")] public string? ExternalUrl { get; set; }
    public string? Perm { get; set; }
    [Column(Name = "always_show")] public int AlwaysShow { get; set; }
    [Column(Name = "keep_alive")] public int KeepAlive { get; set; }
    public int Visible { get; set; } = 1;
    public int Sort { get; set; }
    public string? Icon { get; set; }
    public string? Redirect { get; set; }
    public string? Params { get; set; }

    [Column(Name = "create_time")] public DateTime CreateTime { get; set; }
}