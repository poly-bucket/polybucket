using System;

namespace Core.Models.SystemSettings
{
    public class SystemSetup
    {
        public Guid Id { get; set; }
        public bool IsAdminConfigured { get; set; }
        public bool IsRoleConfigured { get; set; }
        public bool IsModerationConfigured { get; set; }
        public bool RequireUploadModeration { get; set; }
        public string ModeratorRoles { get; set; } // Comma-separated list of role IDs that can moderate

        // Constructor that sets default values
        public SystemSetup()
        {
            Id = Guid.NewGuid();
            IsAdminConfigured = false;
            IsRoleConfigured = false;
            IsModerationConfigured = false;
            RequireUploadModeration = false;
            ModeratorRoles = "";
        }
    }
}