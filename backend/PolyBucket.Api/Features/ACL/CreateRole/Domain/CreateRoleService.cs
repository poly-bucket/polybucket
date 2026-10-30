using PolyBucket.Api.Features.ACL.CreateRole.Http;
using PolyBucket.Api.Features.ACL.GetAllRoles.Http;
using PolyBucket.Api.Features.ACL.GetRole.Domain;
using PolyBucket.Api.Features.ACL.Services;

namespace PolyBucket.Api.Features.ACL.CreateRole.Domain;

public class CreateRoleService(IRoleManagementService roleManagement) : ICreateRoleService
{
    public async Task<(CreateRoleOutcome Outcome, RoleDto? Role, string? Error)> CreateRoleAsync(
        Guid actorUserId, CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty)
        {
            return (CreateRoleOutcome.Unauthorized, null, null);
        }

        try
        {
            var role = new Role
            {
                Name = request.Name,
                Description = request.Description,
                Priority = request.Priority,
                IsSystemRole = request.IsSystemRole,
                IsDefault = request.IsDefault,
                ParentRoleId = request.ParentRoleId,
                IsActive = true,
                CanBeDeleted = !request.IsSystemRole,
                Color = request.Color
            };
            var created = await roleManagement.CreateRoleAsync(role, request.InitialPermissions);
            var dto = await RoleDtoMapper.MapAsync(created, roleManagement);
            return (CreateRoleOutcome.Success, dto, null);
        }
        catch (InvalidOperationException ex)
        {
            return (CreateRoleOutcome.Invalid, null, ex.Message);
        }
        catch (Exception ex)
        {
            return (CreateRoleOutcome.Invalid, null, $"Failed to create role: {ex.Message}");
        }
    }
}
