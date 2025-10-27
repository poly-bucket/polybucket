using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Repository
{
    public class FederationRepository(PolyBucketDbContext context) : IFederationRepository
    {
        private readonly PolyBucketDbContext _context = context;

        #region Federation Settings

        public async Task<FederationSettings?> GetFederationSettingsAsync()
        {
            return await _context.FederationSettings.FirstOrDefaultAsync();
        }

        public async Task<FederationSettings> UpdateFederationSettingsAsync(FederationSettings settings)
        {
            var existing = await _context.FederationSettings.FirstOrDefaultAsync();
            
            if (existing == null)
            {
                settings.Id = Guid.NewGuid();
                _context.FederationSettings.Add(settings);
            }
            else
            {
                existing.IsFederationEnabled = settings.IsFederationEnabled;
                existing.InstanceName = settings.InstanceName;
                existing.InstanceDescription = settings.InstanceDescription;
                existing.AdminContact = settings.AdminContact;
                existing.BaseUrl = settings.BaseUrl;
                existing.PrivateKey = settings.PrivateKey;
                existing.PublicKey = settings.PublicKey;
                existing.KeyRotationIntervalDays = settings.KeyRotationIntervalDays;
                existing.LastKeyRotation = settings.LastKeyRotation;
                existing.RequireHttps = settings.RequireHttps;
                existing.AllowSelfSignedCertificates = settings.AllowSelfSignedCertificates;
                existing.HandshakeTimeoutMinutes = settings.HandshakeTimeoutMinutes;
                existing.DefaultSyncIntervalMinutes = settings.DefaultSyncIntervalMinutes;
                existing.MaxConcurrentSyncs = settings.MaxConcurrentSyncs;
                existing.MaxModelsPerInstance = settings.MaxModelsPerInstance;
                existing.MaxModelFileSizeBytes = settings.MaxModelFileSizeBytes;
                existing.ModelCacheRetentionDays = settings.ModelCacheRetentionDays;
                existing.AutoAcceptHandshakes = settings.AutoAcceptHandshakes;
                existing.SharePublicModelsOnly = settings.SharePublicModelsOnly;
                existing.ShareFeaturedModelsOnly = settings.ShareFeaturedModelsOnly;
                existing.AllowedFileTypes = settings.AllowedFileTypes;
                existing.BlockedCategories = settings.BlockedCategories;
                existing.AllowNSFWContent = settings.AllowNSFWContent;
                existing.RequireApprovalForNewInstances = settings.RequireApprovalForNewInstances;
                existing.MaxRequestsPerMinute = settings.MaxRequestsPerMinute;
                existing.MaxDownloadsPerHour = settings.MaxDownloadsPerHour;
                existing.MaxBandwidthPerDay = settings.MaxBandwidthPerDay;
                existing.HeartbeatIntervalMinutes = settings.HeartbeatIntervalMinutes;
                existing.HealthCheckTimeoutSeconds = settings.HealthCheckTimeoutSeconds;
                existing.EnableMetrics = settings.EnableMetrics;
                existing.EnableDetailedLogging = settings.EnableDetailedLogging;
                existing.FederationInviteUrl = settings.FederationInviteUrl;
                existing.InviteUrlGeneratedAt = settings.InviteUrlGeneratedAt;
                existing.InviteUrlExpiresAt = settings.InviteUrlExpiresAt;
                existing.EnableDiscovery = settings.EnableDiscovery;
                existing.DiscoveryTags = settings.DiscoveryTags;
                existing.EnableAutomaticBackup = settings.EnableAutomaticBackup;
                existing.BackupRetentionDays = settings.BackupRetentionDays;
                existing.LastBackupAt = settings.LastBackupAt;
                existing.FederationProtocolVersion = settings.FederationProtocolVersion;
                existing.SupportedVersions = settings.SupportedVersions;
                existing.TotalConnectedInstances = settings.TotalConnectedInstances;
                existing.TotalModelsShared = settings.TotalModelsShared;
                existing.TotalModelsReceived = settings.TotalModelsReceived;
                existing.TotalBytesTransferred = settings.TotalBytesTransferred;
                existing.LastStatsUpdate = settings.LastStatsUpdate;
                
                settings = existing;
            }
            
            await _context.SaveChangesAsync();
            return settings;
        }

        #endregion

        #region Federated Instances

        public async Task<FederatedInstance?> GetFederatedInstanceAsync(Guid id)
        {
            return await _context.FederatedInstances
                .Include(f => f.SharedModels)
                .Include(f => f.Handshakes)
                .Include(f => f.AuditLogs)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<FederatedInstance>> GetFederatedInstancesAsync()
        {
            return await _context.FederatedInstances
                .Include(f => f.SharedModels)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<FederatedInstance> AddFederatedInstanceAsync(FederatedInstance instance)
        {
            instance.CreatedAt = DateTime.UtcNow;
            instance.UpdatedAt = DateTime.UtcNow;
            
            _context.FederatedInstances.Add(instance);
            await _context.SaveChangesAsync();
            
            return instance;
        }

        public async Task<FederatedInstance> UpdateFederatedInstanceAsync(FederatedInstance instance)
        {
            instance.UpdatedAt = DateTime.UtcNow;
            
            _context.FederatedInstances.Update(instance);
            await _context.SaveChangesAsync();
            
            return instance;
        }

        public async Task DeleteFederatedInstanceAsync(Guid id)
        {
            var instance = await _context.FederatedInstances.FindAsync(id);
            if (instance != null)
            {
                _context.FederatedInstances.Remove(instance);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Federated Models

        public async Task<FederatedModel?> GetFederatedModelAsync(Guid id)
        {
            return await _context.FederatedModels
                .Include(f => f.FederatedInstance)
                .Include(f => f.LocalModel)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<FederatedModel>> GetFederatedModelsAsync(Guid instanceId)
        {
            return await _context.FederatedModels
                .Include(f => f.FederatedInstance)
                .Include(f => f.LocalModel)
                .Where(f => f.FederatedInstanceId == instanceId)
                .OrderByDescending(f => f.LastSyncAt)
                .ToListAsync();
        }

        public async Task<FederatedModel> AddFederatedModelAsync(FederatedModel model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            
            _context.FederatedModels.Add(model);
            await _context.SaveChangesAsync();
            
            return model;
        }

        public async Task<FederatedModel> UpdateFederatedModelAsync(FederatedModel model)
        {
            model.UpdatedAt = DateTime.UtcNow;
            
            _context.FederatedModels.Update(model);
            await _context.SaveChangesAsync();
            
            return model;
        }

        public async Task DeleteFederatedModelAsync(Guid id)
        {
            var model = await _context.FederatedModels.FindAsync(id);
            if (model != null)
            {
                _context.FederatedModels.Remove(model);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Handshakes

        public async Task<FederationHandshake?> GetHandshakeAsync(Guid id)
        {
            return await _context.FederationHandshakes
                .Include(h => h.InitiatorInstance)
                .Include(h => h.ResponderInstance)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<IEnumerable<FederationHandshake>> GetHandshakesAsync(Guid instanceId)
        {
            return await _context.FederationHandshakes
                .Include(h => h.InitiatorInstance)
                .Include(h => h.ResponderInstance)
                .Where(h => h.InitiatorInstanceId == instanceId || h.ResponderInstanceId == instanceId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<FederationHandshake> AddHandshakeAsync(FederationHandshake handshake)
        {
            handshake.CreatedAt = DateTime.UtcNow;
            handshake.UpdatedAt = DateTime.UtcNow;
            
            _context.FederationHandshakes.Add(handshake);
            await _context.SaveChangesAsync();
            
            return handshake;
        }

        public async Task<FederationHandshake> UpdateHandshakeAsync(FederationHandshake handshake)
        {
            handshake.UpdatedAt = DateTime.UtcNow;
            
            _context.FederationHandshakes.Update(handshake);
            await _context.SaveChangesAsync();
            
            return handshake;
        }

        #endregion

        #region Audit Logs

        public async Task<FederationAuditLog> AddAuditLogAsync(FederationAuditLog auditLog)
        {
            auditLog.CreatedAt = DateTime.UtcNow;
            auditLog.UpdatedAt = DateTime.UtcNow;
            
            _context.FederationAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            
            return auditLog;
        }

        public async Task<IEnumerable<FederationAuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50)
        {
            return await _context.FederationAuditLogs
                .Include(a => a.FederatedInstance)
                .Include(a => a.User)
                .OrderByDescending(a => a.EventTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<FederationAuditLog>> GetAuditLogsForInstanceAsync(Guid instanceId, int page = 1, int pageSize = 50)
        {
            return await _context.FederationAuditLogs
                .Include(a => a.FederatedInstance)
                .Include(a => a.User)
                .Where(a => a.FederatedInstanceId == instanceId)
                .OrderByDescending(a => a.EventTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        #endregion
    }
} 