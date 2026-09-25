using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_notice")]
public class SysNotice
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    public string? Title { get; set; }
    public string? Content { get; set; }
    public int Type { get; set; }
    public string Level { get; set; } = "";
    [Column(Name = "target_type")] public int TargetType { get; set; }
    [Column(Name = "target_user_ids")] public string? TargetUserIds { get; set; }
    [Column(Name = "publisher_id")] public long? PublisherId { get; set; }
    [Column(Name = "publish_status")] public int PublishStatus { get; set; }
    [Column(Name = "publish_time")] public DateTime? PublishTime { get; set; }
    [Column(Name = "revoke_time")] public DateTime? RevokeTime { get; set; }
    [Column(Name = "create_by")] public long CreateBy { get; set; }
    [Column(Name = "update_by")] public long? UpdateBy { get; set; }
    [Column(Name = "is_deleted")] public int IsDeleted { get; set; }
    [Column(Name = "create_time")] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    [Column(Name = "update_time")] public DateTime? UpdateTime { get; set; }
}