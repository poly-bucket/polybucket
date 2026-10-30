namespace PolyBucket.Api.Features.ACL.AssignUserRole.Repository;

public interface IAssignUserRoleRepository
{
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class AssignUserRoleRepository(global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IAssignUserRoleRepository
{
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
