using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Results;
using ECommerce.Shared.Utils;

namespace ECommerce.Application.Features.SystemManage;

/// <summary>用户管理：分页、新增、编辑、删除、重置密码、选项。</summary>
public class SysUserService
{
    private readonly ISysDeptRepository _depts;
    private readonly ISysRoleRepository _roles;
    private readonly ISysUserRoleRepository _userRoles;
    private readonly ISysUserRepository _users;

    public SysUserService(ISysUserRepository users, ISysDeptRepository depts, ISysRoleRepository roles,
        ISysUserRoleRepository userRoles)
    {
        _users = users;
        _depts = depts;
        _roles = roles;
        _userRoles = userRoles;
    }

    public async Task<(List<object> List, long Total)> GetPageAsync(SysUserQuery q, CancellationToken ct)
    {
        var (items, total) =
            await _users.QueryPageAsync(q.Keywords, q.DeptId, q.BeginTime, q.EndTime, q.PageNum, q.PageSize, ct);

        // 批量查部门名和角色名（数据量小，一次查全量做映射）
        var deptMap = (await _depts.GetAllAsync(ct)).ToDictionary(d => d.Id, d => d.Name);
        var roleMap = (await _roles.GetAllAsync(ct)).ToDictionary(r => r.Id, r => r.Name);

        var list = new List<object>();
        foreach (var u in items)
        {
            var roleIds = await _userRoles.GetRoleIdsByUserIdAsync(u.Id, ct);
            var roleNames = string.Join(",", roleIds.Where(roleMap.ContainsKey).Select(id => roleMap[id]));
            list.Add(new
            {
                id = u.Id.ToString(), username = u.Username, nickname = u.Nickname, mobile = u.Mobile,
                email = u.Email, avatar = u.Avatar, gender = u.Gender, status = u.Status,
                deptName = u.DeptId.HasValue && deptMap.TryGetValue(u.DeptId.Value, out var dn) ? dn : "",
                roleNames,
                createTime = u.CreateTime.ToString("yyyy/MM/dd HH:mm")
            });
        }

        return (list, total);
    }

    public async Task<object?> GetFormAsync(long id, CancellationToken ct)
    {
        var u = await _users.GetByIdAsync(id, ct) ?? throw new BusinessException("用户不存在", ResultCodes.NotFound);
        var roles = await _userRoles.GetRoleIdsByUserIdAsync(id, ct);
        return new
        {
            id = u.Id.ToString(), username = u.Username, nickname = u.Nickname, mobile = u.Mobile,
            email = u.Email, avatar = u.Avatar, gender = u.Gender, status = u.Status,
            deptId = u.DeptId?.ToString(), roleIds = roles.Select(r => r.ToString()).ToList()
        };
    }

    public async Task CreateAsync(SysUserDto dto, CancellationToken ct)
    {
        if (await _users.ExistsUsernameAsync(dto.Username, null, ct))
            throw new BusinessException("用户名已存在", ResultCodes.Conflict);
        var user = new SysUser
        {
            Username = dto.Username, Nickname = dto.Nickname, Mobile = dto.Mobile, Email = dto.Email,
            Avatar = dto.Avatar, Gender = dto.Gender, Status = dto.Status,
            DeptId = long.TryParse(dto.DeptId, out var d) ? d : null,
            Password = PasswordHasher.Hash("123456"),
            IsDeleted = 0, CreateTime = DateTime.UtcNow
        };
        var roleIds = (dto.RoleIds ?? new List<string>()).Where(s => long.TryParse(s, out _)).Select(long.Parse)
            .Distinct();
        await _users.CreateWithRolesAsync(user, roleIds, ct);
    }

    public async Task UpdateAsync(long id, SysUserDto dto, CancellationToken ct)
    {
        var u = await _users.GetByIdAsync(id, ct) ?? throw new BusinessException("用户不存在", ResultCodes.NotFound);
        u.Username = dto.Username;
        u.Nickname = dto.Nickname;
        u.Mobile = dto.Mobile;
        u.Email = dto.Email;
        u.Avatar = dto.Avatar;
        u.Gender = dto.Gender;
        u.Status = dto.Status;
        u.DeptId = long.TryParse(dto.DeptId, out var d) ? d : null;
        u.UpdateTime = DateTime.UtcNow;
        var roleIds = (dto.RoleIds ?? new List<string>()).Where(s => long.TryParse(s, out _)).Select(long.Parse)
            .Distinct();
        await _users.UpdateWithRolesAsync(u, roleIds, ct);
    }

    public async Task DeleteAsync(string ids, CancellationToken ct)
    {
        var list = ParseIds(ids);
        await _users.DeleteByIdsAsync(list, ct);
    }

    public async Task ResetPasswordAsync(long id, string password, CancellationToken ct)
    {
        if (!await _users.UpdatePasswordAsync(id, PasswordHasher.Hash(password), ct))
            throw new BusinessException("用户不存在", ResultCodes.NotFound);
    }

    public Task<List<SysOptionDto>> GetOptionsAsync(CancellationToken ct)
    {
        return _users.GetActiveUsersAsync(ct)
            .ContinueWith(
                t => t.Result
                    .Select(u => new SysOptionDto { Value = u.Id.ToString(), Label = u.Nickname ?? u.Username })
                    .ToList(), ct);
    }

    internal static List<long> ParseIds(string ids)
    {
        return ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => long.TryParse(s, out _)).Select(long.Parse).Distinct().ToList();
    }
}