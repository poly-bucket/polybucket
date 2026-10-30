namespace PolyBucket.Api.Features.ACL.GetUserPermissionCatalog.Repository;

public interface IGetUserPermissionCatalogRepository
{
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class GetUserPermissionCatalogRepository(global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IGetUserPermissionCatalogRepository
{
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
