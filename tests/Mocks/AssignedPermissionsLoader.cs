
using Microsoft.Extensions.Options;

namespace NGroot.Tests;


public class AssignedPermissionsLoader : ModelLoader<AssignedPermission>
{
    public AssignedPermissionsLoader(IFileLoader fileLoader, IOptions<NgrootSettings> settings, IAssignedPermissionsRepository assignedPermissionsRepository) : base(settings)
    {
        Setup("AssignedPermissions")
        .FindDuplicatesWith(m => assignedPermissionsRepository.GetByPermissionAndRoleAsync(m.PermissionId, m.RoleId))
        .CreateModelUsing(m => assignedPermissionsRepository.CreateAsync(m))
        .UseFileLoader(() => fileLoader)
        // Permission Map
        .With<Permission>("Permissions", m => m.PermissionId,
        permission => permission.Id,
        (permission, m) => m.Permission?.Name == permission.Name,
        (permission, m) => m.Permission = null)
        // Role Map
        .With<Role>("Roles", d => d.RoleId, s => s.Id,
        (role, m) => role.Name == m.Role?.Name,
        (role, m) => m.Role = null);
    }
}
