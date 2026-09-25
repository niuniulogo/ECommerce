using FreeSql.DataAnnotations;

namespace ECommerce.Domain.Entities.System;

[Table(Name = "sys_log")]
public class SysLog
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    [Column(Name = "operator_id")] public long? OperatorId { get; set; }
    [Column(Name = "operator_name")] public string? OperatorName { get; set; }
    [Column(Name = "request_uri")] public string? RequestUri { get; set; }
    [Column(Name = "request_method")] public string? RequestMethod { get; set; }
    public string? Ip { get; set; }
    public string? Browser { get; set; }
    public string? Os { get; set; }
    public int Status { get; set; } = 1;
    [Column(Name = "error_msg")] public string? ErrorMsg { get; set; }
    [Column(Name = "execution_time")] public int? ExecutionTime { get; set; }
    [Column(Name = "create_time")] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}