using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class ModelRejectRequest
    {
        public string Reason { get; set; }
    }

    public class ModerationSettings
    {
        public bool RequireUploadModeration { get; set; }
        public string ModeratorRoles { get; set; }
    }

    public class ModelModerationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid ModelId { get; set; }
    }
}