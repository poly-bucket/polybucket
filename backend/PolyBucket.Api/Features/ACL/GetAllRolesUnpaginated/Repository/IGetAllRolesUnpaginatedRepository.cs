namespace PolyBucket.Api.Features.ACL.GetAllRolesUnpaginated.Repository;

public interface IGetAllRolesUnpaginatedRepository
{
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; }
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class GetAllRolesUnpaginatedRepository(
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository roles,
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : IGetAllRolesUnpaginatedRepository
{
    public global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; } = roles;
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
