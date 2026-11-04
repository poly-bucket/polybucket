using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;

namespace PolyBucket.Database.Entities.Authentication;

public class AccountRecoveryCode : Auditable
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string ProtectedCodeHash { get; set; } = null!;
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public int Version { get; set; } = 1;
}
