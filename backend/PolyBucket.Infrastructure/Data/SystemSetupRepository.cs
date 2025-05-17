using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Core.Interfaces;

namespace Infrastructure.Data
{
    public class SystemSetupRepository : ISystemSetupRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<SystemSetupRepository> _logger;

        public SystemSetupRepository(DatabaseContext context, ILogger<SystemSetupRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SystemSetup> GetSetupStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the system setup entry or create a new one if it doesn't exist
                var setup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
                if (setup == null)
                {
                    setup = new SystemSetup();
                    _context.SystemSetups.Add(setup);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                
                return setup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system setup status");
                throw;
            }
        }

        public async Task<SystemSetup> UpdateSetupStatusAsync(SystemSetup systemSetup, CancellationToken cancellationToken = default)
        {
            try
            {
                systemSetup.UpdatedAt = DateTime.UtcNow;
                _context.SystemSetups.Update(systemSetup);
                await _context.SaveChangesAsync(cancellationToken);
                return systemSetup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system setup status");
                throw;
            }
        }

        public async Task<bool> IsAdminConfiguredAsync(CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            return setup.IsAdminConfigured;
        }

        public async Task<bool> IsRoleConfiguredAsync(CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            return setup.IsRoleConfigured;
        }

        public async Task SetAdminConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            setup.IsAdminConfigured = isConfigured;
            await UpdateSetupStatusAsync(setup, cancellationToken);
        }

        public async Task SetRoleConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            setup.IsRoleConfigured = isConfigured;
            await UpdateSetupStatusAsync(setup, cancellationToken);
        }

        public async Task<bool> RequireUploadModerationAsync(CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            return setup.RequireUploadModeration;
        }

        public async Task SetRequireUploadModerationAsync(bool requireModeration, CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            setup.RequireUploadModeration = requireModeration;
            await UpdateSetupStatusAsync(setup, cancellationToken);
        }

        public async Task<string> GetModeratorRolesAsync(CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            return setup.ModeratorRoles;
        }

        public async Task SetModeratorRolesAsync(string moderatorRoles, CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            setup.ModeratorRoles = moderatorRoles;
            await UpdateSetupStatusAsync(setup, cancellationToken);
        }

        public async Task<bool> IsModerationConfiguredAsync(CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            return setup.IsModerationConfigured;
        }

        public async Task SetModerationConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default)
        {
            var setup = await GetSetupStatusAsync(cancellationToken);
            setup.IsModerationConfigured = isConfigured;
            await UpdateSetupStatusAsync(setup, cancellationToken);
        }
    }
} 