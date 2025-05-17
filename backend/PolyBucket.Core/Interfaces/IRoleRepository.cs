using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Models.Roles;

namespace Core.Interfaces
{
    public interface IRoleRepository
    {
        // Read operations
        Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);
        
        // Write operations
        Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
        
        // Helper methods
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}