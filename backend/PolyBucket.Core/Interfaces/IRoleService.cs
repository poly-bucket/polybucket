using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Models.Roles;

namespace Core.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default);

        Task<Role> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Role> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

        Task<Role> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    }
}