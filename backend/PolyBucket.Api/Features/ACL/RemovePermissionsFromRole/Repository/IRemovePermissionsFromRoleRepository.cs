namespace PolyBucket.Api.Features.ACL.RemovePermissionsFromRole.Repository;

public interface IRemovePermissionsFromRoleRepository
{
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; }
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class RemovePermissionsFromRoleRepository(
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository roles,
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IRemovePermissionsFromRoleRepository
{
    public global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; } = roles;
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
