using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;
using System.Collections.Generic;

namespace PolyBucket.Database.Entities.Authentication
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

}
 