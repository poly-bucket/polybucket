namespace PolyBucket.Api.Features.ACL.RevokeUserPermission.Repository;

public interface IRevokeUserPermissionRepository
{
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class RevokeUserPermissionRepository(global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IRevokeUserPermissionRepository
{
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
