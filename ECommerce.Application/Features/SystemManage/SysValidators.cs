using FluentValidation;

namespace ECommerce.Application.Features.SystemManage;

// ==================== 认证 ====================
public class SysLoginDtoValidator : AbstractValidator<SysLoginDto>
{
    public SysLoginDtoValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(x => x.Password).NotEmpty().WithMessage("密码不能为空");
        RuleFor(x => x.CaptchaId).NotEmpty().WithMessage("验证码ID不能为空");
        RuleFor(x => x.CaptchaCode).NotEmpty().WithMessage("验证码答案不能为空");
    }
}

public class SysPasswordChangeDtoValidator : AbstractValidator<SysPasswordChangeDto>
{
    public SysPasswordChangeDtoValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("原密码不能为空");
        RuleFor(x => x.NewPassword).MinimumLength(6).WithMessage("新密码至少 6 位");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("两次输入的密码不一致");
    }
}

public class SysProfileUpdateDtoValidator : AbstractValidator<SysProfileUpdateDto>
{
    public SysProfileUpdateDtoValidator()
    {
        RuleFor(x => x.Nickname).MaximumLength(20).WithMessage("昵称最长 20 字").When(x => x.Nickname != null);
    }
}

// ==================== 用户 ====================
public class SysUserDtoValidator : AbstractValidator<SysUserDto>
{
    public SysUserDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名至少 3 位")
            .MaximumLength(32).WithMessage("用户名最长 32 位");
        RuleFor(x => x.Nickname).NotEmpty().WithMessage("昵称不能为空");
        RuleFor(x => x.Mobile).Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Mobile));
        RuleFor(x => x.Email).EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class SysResetPasswordDtoValidator : AbstractValidator<SysResetPasswordDto>
{
    public SysResetPasswordDtoValidator()
    {
        RuleFor(x => x.Password).MinimumLength(6).WithMessage("密码至少 6 位");
    }
}

// ==================== 角色 ====================
public class SysRoleFormDtoValidator : AbstractValidator<SysRoleFormDto>
{
    public SysRoleFormDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("角色名称不能为空")
            .MaximumLength(32).WithMessage("角色名称最长 32 字");
        RuleFor(x => x.Code).NotEmpty().WithMessage("角色编码不能为空")
            .MaximumLength(64).WithMessage("角色编码最长 64 字");
    }
}

// ==================== 菜单 ====================
public class SysMenuFormDtoValidator : AbstractValidator<SysMenuFormDto>
{
    public SysMenuFormDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("菜单名称不能为空");
        RuleFor(x => x.Type).Must(t => t is "M" or "C" or "B").WithMessage("类型只能是 M(目录)/C(菜单)/B(按钮)");
    }
}

// ==================== 部门 ====================
public class SysDeptFormDtoValidator : AbstractValidator<SysDeptFormDto>
{
    public SysDeptFormDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("部门名称不能为空");
        RuleFor(x => x.Code).NotEmpty().WithMessage("部门编号不能为空");
    }
}

// ==================== 字典 ====================
public class SysDictTypeFormDtoValidator : AbstractValidator<SysDictTypeFormDto>
{
    public SysDictTypeFormDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("字典名称不能为空");
        RuleFor(x => x.DictCode).NotEmpty().WithMessage("字典编码不能为空")
            .Matches("^[a-z][a-z0-9_]*$").WithMessage("字典编码只能用小写字母、数字、下划线，且以字母开头");
    }
}

public class SysDictItemFormDtoValidator : AbstractValidator<SysDictItemFormDto>
{
    public SysDictItemFormDtoValidator()
    {
        RuleFor(x => x.Label).NotEmpty().WithMessage("字典项标签不能为空");
        RuleFor(x => x.Value).NotEmpty().WithMessage("字典项值不能为空");
    }
}

// ==================== 通知 ====================
public class SysNoticeFormDtoValidator : AbstractValidator<SysNoticeFormDto>
{
    public SysNoticeFormDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("通知标题不能为空")
            .MaximumLength(100).WithMessage("标题最长 100 字");
        RuleFor(x => x.Content).NotEmpty().WithMessage("通知内容不能为空");
        RuleFor(x => x.TargetType).Must(t => t is 1 or 2).WithMessage("目标类型只能是 1(全体)/2(指定)");
    }
}

// ==================== 配置 ====================
public class SysConfigFormDtoValidator : AbstractValidator<SysConfigFormDto>
{
    public SysConfigFormDtoValidator()
    {
        RuleFor(x => x.ConfigName).NotEmpty().WithMessage("配置名称不能为空");
        RuleFor(x => x.ConfigKey).NotEmpty().WithMessage("配置键不能为空");
        RuleFor(x => x.ConfigValue).NotEmpty().WithMessage("配置值不能为空");
    }
}

// ==================== Refresh Token ====================
public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("refresh token 不能为空");
    }
}