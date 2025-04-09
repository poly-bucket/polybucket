using System;

namespace PolyBucket.Core.Models
{
    public class SystemSettings
    {
        public bool IsAdminConfigured { get; set; }
        public bool IsRoleConfigured { get; set; }
        public bool IsModerationConfigured { get; set; }
        public bool RequireUploadModeration { get; set; }
        public string ModeratorRoles { get; set; }
    }
} 