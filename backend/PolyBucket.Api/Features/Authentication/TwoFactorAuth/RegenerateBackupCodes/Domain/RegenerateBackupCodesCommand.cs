using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.RegenerateBackupCodes.Domain
{
    public class RegenerateBackupCodesCommand
    {
        public Guid UserId { get; set; }
    }

    public class RegenerateBackupCodesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public IEnumerable<string>? BackupCodes { get; set; }
    }
} 