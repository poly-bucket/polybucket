using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Interfaces;

namespace PolyBucket.Infrastructure.Data.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .Where(r => r.IsSystemRole)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default)
        {
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return role;
        }

        public async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            role.UpdatedAt = DateTime.UtcNow;
            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return role;
        }

        public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (role == null)
                return false;

            // Soft delete
            role.DeletedAt = DateTime.UtcNow;
            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .AnyAsync(r => r.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .AnyAsync(r => r.Id == id, cancellationToken);
        }
    }
}