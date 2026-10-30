namespace PolyBucket.Api.Features.ACL.GetAllRoles.Repository;

public interface IGetAllRolesRepository
{
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; }
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class GetAllRolesRepository(
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository roles,
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IGetAllRolesRepository
{
    public global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; } = roles;
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
