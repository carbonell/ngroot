
using System;
using System.Threading.Tasks;

namespace NGroot.Tests;


public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Permission(string name)
    {
        Name = name;
    }
}

public class AssignedPermission
{
    public Guid Id { get; set; }

    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public int PermissionId { get; set; }
    public Permission? Permission { get; set; }
}

public interface IAssignedPermissionsRepository
{
    Task<AssignedPermission?> CreateAsync(AssignedPermission assignedPermission);
    Task<AssignedPermission?> GetByPermissionAndRoleAsync(int permissionId, int roleId);

}




