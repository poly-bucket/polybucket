using System;

namespace PolyBucket.Core.Entities
{
    public class SystemSetup
    {
        public Guid Id { get; set; }
        public bool IsAdminConfigured { get; set; }
        public bool IsRoleConfigured { get; set; }
        public bool IsModerationConfigured { get; set; }
        public bool RequireUploadModeration { get; set; }
        public string ModeratorRoles { get; set; } // Comma-separated list of role IDs that can moderate
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Constructor that sets default values
        public SystemSetup()
        {
            Id = Guid.NewGuid();
            IsAdminConfigured = false;
            IsRoleConfigured = false;
            IsModerationConfigured = false;
            RequireUploadModeration = false;
            ModeratorRoles = "";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 