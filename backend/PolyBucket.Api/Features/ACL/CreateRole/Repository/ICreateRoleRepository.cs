namespace PolyBucket.Api.Features.ACL.CreateRole.Repository;

public interface ICreateRoleRepository
{
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; }
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class CreateRoleRepository(
    global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository roles,
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : ICreateRoleRepository
{
    public global::PolyBucket.Api.Features.ACL.Repository.IRoleRepository Roles { get; } = roles;
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
