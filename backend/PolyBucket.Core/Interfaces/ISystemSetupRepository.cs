using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ISystemSetupRepository
    {
        Task<SystemSetup> GetSetupStatusAsync(CancellationToken cancellationToken = default);
        Task<SystemSetup> UpdateSetupStatusAsync(SystemSetup systemSetup, CancellationToken cancellationToken = default);
        Task<bool> IsAdminConfiguredAsync(CancellationToken cancellationToken = default);
        Task<bool> IsRoleConfiguredAsync(CancellationToken cancellationToken = default);
        Task SetAdminConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default);
        Task SetRoleConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default);
        Task<bool> RequireUploadModerationAsync(CancellationToken cancellationToken = default);
        Task SetRequireUploadModerationAsync(bool requireModeration, CancellationToken cancellationToken = default);
        Task<string> GetModeratorRolesAsync(CancellationToken cancellationToken = default);
        Task SetModeratorRolesAsync(string moderatorRoles, CancellationToken cancellationToken = default);
        Task<bool> IsModerationConfiguredAsync(CancellationToken cancellationToken = default);
        Task SetModerationConfiguredAsync(bool isConfigured, CancellationToken cancellationToken = default);
    }
} 