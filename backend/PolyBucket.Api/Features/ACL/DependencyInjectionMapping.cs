using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL;

public static class AclDependencyInjectionMapping
{
    public static IServiceCollection AddAclFeature(this IServiceCollection services)
    {
        services.AddTransient<Repository.IRoleRepository, Repository.RoleRepository>();
        services.AddTransient<IPermissionService, PermissionService>();
        services.AddTransient<IRoleManagementService, RoleManagementService>();

        services.AddTransient<GetAllRoles.Repository.IGetAllRolesRepository, GetAllRoles.Repository.GetAllRolesRepository>();
        services.AddTransient<GetAllRoles.Domain.IGetAllRolesService, GetAllRoles.Domain.GetAllRolesService>();
        services.AddTransient<GetAllRolesUnpaginated.Domain.IGetAllRolesUnpaginatedService, GetAllRolesUnpaginated.Domain.GetAllRolesUnpaginatedService>();
        services.AddTransient<GetRole.Domain.IGetRoleService, GetRole.Domain.GetRoleService>();
        services.AddTransient<CreateRole.Domain.ICreateRoleService, CreateRole.Domain.CreateRoleService>();
        services.AddTransient<UpdateRole.Domain.IUpdateRoleService, UpdateRole.Domain.UpdateRoleService>();
        services.AddTransient<DeleteRole.Domain.IDeleteRoleService, DeleteRole.Domain.DeleteRoleService>();
        services.AddTransient<AssignPermissionsToRole.Domain.IAssignPermissionsToRoleService, AssignPermissionsToRole.Domain.AssignPermissionsToRoleService>();
        services.AddTransient<RemovePermissionsFromRole.Domain.IRemovePermissionsFromRoleService, RemovePermissionsFromRole.Domain.RemovePermissionsFromRoleService>();
        services.AddTransient<GetRolePermissionsCatalog.Domain.IGetRolePermissionsCatalogService, GetRolePermissionsCatalog.Domain.GetRolePermissionsCatalogService>();

        services.AddTransient<GetCurrentUserPermissions.Domain.IGetCurrentUserPermissionsService, GetCurrentUserPermissions.Domain.GetCurrentUserPermissionsService>();
        services.AddTransient<GetUserPermissions.Domain.IGetUserPermissionsService, GetUserPermissions.Domain.GetUserPermissionsService>();
        services.AddTransient<AssignUserRole.Domain.IAssignUserRoleService, AssignUserRole.Domain.AssignUserRoleService>();
        services.AddTransient<GrantUserPermission.Domain.IGrantUserPermissionService, GrantUserPermission.Domain.GrantUserPermissionService>();
        services.AddTransient<RevokeUserPermission.Domain.IRevokeUserPermissionService, RevokeUserPermission.Domain.RevokeUserPermissionService>();
        services.AddTransient<CanUserModerateUser.Domain.ICanUserModerateUserService, CanUserModerateUser.Domain.CanUserModerateUserService>();
        services.AddTransient<GetUserPermissionCatalog.Domain.IGetUserPermissionCatalogService, GetUserPermissionCatalog.Domain.GetUserPermissionCatalogService>();

        RegisterRoleActionRepositories(services);
        RegisterPermissionActionRepositories(services);

        return services;
    }

    private static void RegisterRoleActionRepositories(IServiceCollection services)
    {
        services.AddTransient<GetAllRoles.Repository.IGetAllRolesRepository, GetAllRoles.Repository.GetAllRolesRepository>();
        services.AddTransient<GetAllRolesUnpaginated.Repository.IGetAllRolesUnpaginatedRepository, GetAllRolesUnpaginated.Repository.GetAllRolesUnpaginatedRepository>();
        services.AddTransient<GetRole.Repository.IGetRoleRepository, GetRole.Repository.GetRoleRepository>();
        services.AddTransient<CreateRole.Repository.ICreateRoleRepository, CreateRole.Repository.CreateRoleRepository>();
        services.AddTransient<UpdateRole.Repository.IUpdateRoleRepository, UpdateRole.Repository.UpdateRoleRepository>();
        services.AddTransient<DeleteRole.Repository.IDeleteRoleRepository, DeleteRole.Repository.DeleteRoleRepository>();
        services.AddTransient<AssignPermissionsToRole.Repository.IAssignPermissionsToRoleRepository, AssignPermissionsToRole.Repository.AssignPermissionsToRoleRepository>();
        services.AddTransient<RemovePermissionsFromRole.Repository.IRemovePermissionsFromRoleRepository, RemovePermissionsFromRole.Repository.RemovePermissionsFromRoleRepository>();
        services.AddTransient<GetRolePermissionsCatalog.Repository.IGetRolePermissionsCatalogRepository, GetRolePermissionsCatalog.Repository.GetRolePermissionsCatalogRepository>();
    }

    private static void RegisterPermissionActionRepositories(IServiceCollection services)
    {
        services.AddTransient<GetCurrentUserPermissions.Repository.IGetCurrentUserPermissionsRepository, GetCurrentUserPermissions.Repository.GetCurrentUserPermissionsRepository>();
        services.AddTransient<GetUserPermissions.Repository.IGetUserPermissionsRepository, GetUserPermissions.Repository.GetUserPermissionsRepository>();
        services.AddTransient<AssignUserRole.Repository.IAssignUserRoleRepository, AssignUserRole.Repository.AssignUserRoleRepository>();
        services.AddTransient<GrantUserPermission.Repository.IGrantUserPermissionRepository, GrantUserPermission.Repository.GrantUserPermissionRepository>();
        services.AddTransient<RevokeUserPermission.Repository.IRevokeUserPermissionRepository, RevokeUserPermission.Repository.RevokeUserPermissionRepository>();
        services.AddTransient<CanUserModerateUser.Repository.ICanUserModerateUserRepository, CanUserModerateUser.Repository.CanUserModerateUserRepository>();
        services.AddTransient<GetUserPermissionCatalog.Repository.IGetUserPermissionCatalogRepository, GetUserPermissionCatalog.Repository.GetUserPermissionCatalogRepository>();
    }
}
