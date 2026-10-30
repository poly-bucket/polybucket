namespace PolyBucket.Api.Features.ACL.AssignPermissionsToRole.Repository;

public interface IAssignPermissionsToRoleRepository
{
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; }
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class AssignPermissionsToRoleRepository(
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository roles,
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IAssignPermissionsToRoleRepository
{
    public global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; } = roles;
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
