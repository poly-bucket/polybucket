namespace PolyBucket.Api.Features.ACL.CanUserModerateUser.Repository;

public interface ICanUserModerateUserRepository
{
    global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; }
}

public class CanUserModerateUserRepository(global::PolyBucket.Api.Features.ACL.Services.IPermissionService permissions) : ICanUserModerateUserRepository
{
    public global::PolyBucket.Api.Features.ACL.Services.IPermissionService Permissions { get; } = permissions;
}
