using ECommerce.Domain.Repositories.System;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Results;
using ECommerce.Shared.Utils;

namespace ECommerce.Application.Features.SystemManage;

/// <summary>认证服务：登录、获取当前用户信息、个人资料、改密。</summary>
public class SysAuthService
{
    private readonly SysJwtService _jwt;
    private readonly ISysUserRoleRepository _userRoles;
    private readonly ISysUserRepository _users;

    public SysAuthService(ISysUserRepository users, ISysUserRoleRepository userRoles, SysJwtService jwt)
    {
        _users = users;
        _userRoles = userRoles;
        _jwt = jwt;
    }

    public async Task<SysLoginResultDto> LoginAsync(string userName, string password, CancellationToken ct)
    {
        var user = await _users.GetByUsernameAsync(userName, ct)
                   ?? throw new BusinessException("用户名或密码错误", ResultCodes.Unauthorized);
        if (user.Status != 1) throw new BusinessException("账号已被禁用", ResultCodes.Forbidden);
        if (!PasswordHasher.Verify(password, user.Password))
            throw new BusinessException("用户名或密码错误", ResultCodes.Unauthorized);

        var access = _jwt.Generate(user.Id, user.Username);
        var refresh = _jwt.Generate(user.Id, user.Username, SysJwtService.RefreshUse);
        return new SysLoginResultDto
            { AccessToken = access, RefreshToken = refresh, ExpiresIn = _jwt.AccessMinutes * 60 };
    }

    public async Task<SysUserInfoDto> GetUserInfoAsync(long userId, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct)
                   ?? throw new BusinessException("用户不存在", ResultCodes.NotFound);
        var (roles, perms) = await _userRoles.GetRoleCodesByUserIdAsync(userId, ct);
        return new SysUserInfoDto
        {
            UserId = user.Id, UserName = user.Username, NickName = user.Nickname, Avatar = user.Avatar, Roles = roles,
            Perms = perms
        };
    }

    public async Task UpdateProfileAsync(long userId, SysProfileUpdateDto dto, CancellationToken ct)
    {
        var u = await _users.GetByIdAsync(userId, ct) ?? throw new BusinessException("用户不存在", ResultCodes.NotFound);
        u.Nickname = dto.Nickname ?? u.Nickname;
        u.Avatar = dto.Avatar ?? u.Avatar;
        if (dto.Gender.HasValue) u.Gender = dto.Gender.Value;
        u.UpdateTime = DateTime.UtcNow;
        await _users.UpdateAsync(u, ct);
    }

    public async Task ChangePasswordAsync(long userId, SysPasswordChangeDto dto, CancellationToken ct)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            throw new BusinessException("两次输入的密码不一致", ResultCodes.ValidationError);
        var u = await _users.GetByIdAsync(userId, ct) ?? throw new BusinessException("用户不存在", ResultCodes.NotFound);
        if (!PasswordHasher.Verify(dto.OldPassword, u.Password))
            throw new BusinessException("原密码不正确", ResultCodes.ValidationError);
        u.Password = PasswordHasher.Hash(dto.NewPassword);
        u.UpdateTime = DateTime.UtcNow;
        await _users.UpdateAsync(u, ct);
    }
}