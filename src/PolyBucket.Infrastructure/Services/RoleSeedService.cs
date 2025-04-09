using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Interfaces;
using PolyBucket.Infrastructure.Data;

namespace PolyBucket.Infrastructure.Services
{
    public class RoleSeedService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleSeedService> _logger;

        public RoleSeedService(
            IRoleRepository roleRepository,
            ILogger<RoleSeedService> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task SeedRolesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Define default roles
                var defaultRoles = new List<(string Name, string Description)>
                {
                    ("Administrator", "Full system access with all privileges"),
                    ("Moderator", "Can moderate content and users"),
                    ("VIP", "Enhanced privileges and features"),
                    ("Banned", "Restricted access to the platform")
                };

                foreach (var (name, description) in defaultRoles)
                {
                    if (!await _roleRepository.ExistsByNameAsync(name, cancellationToken))
                    {
                        _logger.LogInformation("Creating system role: {RoleName}", name);
                        
                        var role = new Role
                        {
                            Name = name,
                            Description = description,
                            IsSystemRole = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _roleRepository.CreateAsync(role, cancellationToken);
                    }
                }

                _logger.LogInformation("System roles have been seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding roles");
                throw;
            }
        }
    }
}