namespace PolyBucket.Api.Features.ACL.GrantUserPermission.Repository;

public interface IGrantUserPermissionRepository
{
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class GrantUserPermissionRepository(global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IGrantUserPermissionRepository
{
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
