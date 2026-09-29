namespace ECommerce.Application.Features.SystemManage;

// ---- 认证 ----
public class SysLoginDto
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string CaptchaCode { get; set; } = "";
    public string CaptchaId { get; set; } = "";
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = "";
}

public class SysLoginResultDto
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
}

public class SysUserInfoDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = "";
    public string? NickName { get; set; }
    public string? Avatar { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Perms { get; set; } = new();
}

public class SysProfileUpdateDto
{
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public int? Gender { get; set; }
}

public class SysPasswordChangeDto
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}

// ---- 用户管理 ----
public class SysUserQuery
{
    public int PageNum { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keywords { get; set; }
    public long? DeptId { get; set; }
    public string[]? CreateTime { get; set; }

    public DateTime? BeginTime =>
        CreateTime is { Length: > 0 } && DateTime.TryParse(CreateTime[0], out var d) ? d : null;

    public DateTime? EndTime => CreateTime is { Length: > 1 } && DateTime.TryParse(CreateTime[1], out var d) ? d : null;
}

public class SysUserDto
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Nickname { get; set; } = "";
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public int Gender { get; set; }
    public int Status { get; set; }
    public string? DeptId { get; set; }
    public List<string>? RoleIds { get; set; }
}

// ---- 部门 ----
public class SysDeptItemDto
{
    public string Id { get; set; } = "";
    public string ParentId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public int Sort { get; set; }
    public int Status { get; set; }
    public List<SysDeptItemDto> Children { get; set; } = new();
}

public class SysDeptFormDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string? ParentId { get; set; }
    public int? Sort { get; set; }
    public int? Status { get; set; }
}

// ---- 角色 ----
public class SysRolePageDto
{
    public string Id { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Sort { get; set; }
    public int Status { get; set; }
    public int? DataScope { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class SysRoleFormDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public int? Sort { get; set; }
    public int? DataScope { get; set; }
    public int? Status { get; set; }
    public List<string>? DeptIds { get; set; }
}

// ---- 菜单 ----
public class SysMenuItemDto
{
    public string Id { get; set; } = "";
    public string ParentId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? RouteName { get; set; }
    public string? RoutePath { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public string? Perm { get; set; }
    public int Sort { get; set; }
    public int Visible { get; set; }
    public List<SysMenuItemDto> Children { get; set; } = new();
}

public class SysMenuFormDto
{
    public string? Id { get; set; }
    public string? ParentId { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "C";
    public string? RouteName { get; set; }
    public string? RoutePath { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public string? Perm { get; set; }
    public int? Sort { get; set; }
    public int? Visible { get; set; }
}

// ---- 字典 ----
public class SysDictTypeDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string DictCode { get; set; } = "";
    public int Status { get; set; }
}

public class SysDictTypeFormDto
{
    public string Name { get; set; } = "";
    public string DictCode { get; set; } = "";
    public int? Status { get; set; }
    public string? Remark { get; set; }
}

public class SysDictItemDto
{
    public string Id { get; set; } = "";
    public string? Label { get; set; } = "";
    public string? Value { get; set; } = "";
    public int Status { get; set; }
    public int Sort { get; set; }
    public string? TagType { get; set; }
}

public class SysDictItemFormDto
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public int? Status { get; set; }
    public int? Sort { get; set; }
    public string? TagType { get; set; }
}

// ---- 通知 ----
public class SysNoticeItemDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public int Type { get; set; }
    public string Level { get; set; } = "";
    public int PublishStatus { get; set; }
    public int IsRead { get; set; }
    public string? PublisherName { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? PublishTime { get; set; }
}

public class SysNoticeFormDto
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public int Type { get; set; }
    public string Level { get; set; } = "";
    public int TargetType { get; set; }
    public List<long>? TargetUserIds { get; set; }
}

// ---- 日志 ----
public class SysLogItemDto
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public int Status { get; set; }
    public string? Ip { get; set; }
    public string? RequestUri { get; set; }
    public string? RequestMethod { get; set; }
    public int? ExecutionTime { get; set; }
    public string? OperatorName { get; set; }
    public string? CreateTime { get; set; }
    public string? Content { get; set; }
    public string? ErrorMsg { get; set; }
}

// ---- 配置 ----
public class SysConfigItemDto
{
    public string Id { get; set; } = "";
    public string ConfigName { get; set; } = "";
    public string ConfigKey { get; set; } = "";
    public string ConfigValue { get; set; } = "";
    public string? Remark { get; set; }
}

public class SysConfigFormDto
{
    public long? Id { get; set; }
    public string ConfigName { get; set; } = "";
    public string ConfigKey { get; set; } = "";
    public string ConfigValue { get; set; } = "";
    public string? Remark { get; set; }
}

// ---- 通用选项 ----
public class SysOptionDto
{
    public string? Value { get; set; }
    public string? Label { get; set; }
    public List<SysOptionDto>? Children { get; set; }
}