using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_user_notice")]
public class SysUserNotice
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    [Column(Name = "notice_id")] public long NoticeId { get; set; }
    [Column(Name = "user_id")] public long UserId { get; set; }
    [Column(Name = "is_read")] public int IsRead { get; set; }
    [Column(Name = "read_time")] public DateTime? ReadTime { get; set; }
    [Column(Name = "is_deleted")] public int IsDeleted { get; set; }
    [Column(Name = "create_time")] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}