namespace PolyBucket.Api.Features.ACL.GetUserPermissions.Repository;

public interface IGetUserPermissionsRepository
{
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class GetUserPermissionsRepository(global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IGetUserPermissionsRepository
{
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
