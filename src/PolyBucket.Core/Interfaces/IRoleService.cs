using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Core.Models.Roles;

namespace PolyBucket.Core.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default);
        Task<RoleDto> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
        Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    }
}