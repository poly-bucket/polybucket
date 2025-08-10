using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Authentication.Domain
{
    public class TwoFactorAuth : Auditable
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public bool IsEnabled { get; set; } = false;
        public DateTime? EnabledAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string? RecoveryEmail { get; set; }
        public virtual ICollection<BackupCode> BackupCodes { get; set; } = new List<BackupCode>();
        
        // CONCURRENCY FIX: Add version field for optimistic concurrency control
        public int Version { get; set; } = 1;
    }

    public class BackupCode : Auditable
    {
        public Guid TwoFactorAuthId { get; set; }
        public virtual TwoFactorAuth TwoFactorAuth { get; set; } = null!;
        public string Code { get; set; } = null!;
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        
        // CONCURRENCY FIX: Add version field for optimistic concurrency control
        public int Version { get; set; } = 1;
    }

    public class TwoFactorAuthSettings
    {
        public bool RequireTwoFactorForAdmins { get; set; } = false;
        public bool AllowBackupCodes { get; set; } = true;
        public int BackupCodeCount { get; set; } = 10;
        public int BackupCodeLength { get; set; } = 8;
        public int TokenExpirySeconds { get; set; } = 30;
        public string IssuerName { get; set; } = "PolyBucket";
        public bool RequireRecoveryEmail { get; set; } = false;
    }
} 