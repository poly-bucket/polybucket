namespace PolyBucket.Api.Features.ACL.GetCurrentUserPermissions.Repository;

public interface IGetCurrentUserPermissionsRepository
{
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class GetCurrentUserPermissionsRepository(global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IGetCurrentUserPermissionsRepository
{
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
