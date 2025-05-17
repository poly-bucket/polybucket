using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Models.Roles;
using Microsoft.Extensions.Logging;
using Core.Interfaces;

using Core.Models.Roles;

namespace Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _roleRepository.GetAllAsync(cancellationToken);
            return roles.Select(MapToDto);
        }

        public async Task<RoleDto> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
            if (role == null)
                return null;

            return MapToDto(role);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
        {
            // Check if role with the same name already exists
            if (await _roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }

            var role = new Role
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdRole = await _roleRepository.CreateAsync(role, cancellationToken);
            _logger.LogInformation("Role created: {roleName} with ID {roleId}", createdRole.Name, createdRole.Id);

            return MapToDto(createdRole);
        }

        public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {id} not found");

            // Do not allow modifying system roles
            if (role.IsSystemRole)
                throw new InvalidOperationException("System roles cannot be modified");

            // Check if new name is unique (if name is being changed)
            if (!string.IsNullOrEmpty(request.Name) && request.Name != role.Name)
            {
                if (await _roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
                    throw new InvalidOperationException($"Role with name '{request.Name}' already exists");

                role.Name = request.Name;
            }

            // Update description if provided
            if (request.Description != null)
                role.Description = request.Description;

            role.UpdatedAt = DateTime.UtcNow;

            var updatedRole = await _roleRepository.UpdateAsync(role, cancellationToken);
            _logger.LogInformation("Role updated: {roleName} with ID {roleId}", updatedRole.Name, updatedRole.Id);

            return MapToDto(updatedRole);
        }

        public async Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
            if (role == null)
                return false;

            // Do not allow deleting system roles
            if (role.IsSystemRole)
                throw new InvalidOperationException("System roles cannot be deleted");

            var result = await _roleRepository.SoftDeleteAsync(id, cancellationToken);
            if (result)
                _logger.LogInformation("Role soft deleted: {roleName} with ID {roleId}", role.Name, role.Id);

            return result;
        }

        // Helper method to map from entity to DTO
        private static RoleDto MapToDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
    }
}