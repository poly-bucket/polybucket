using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using System;

namespace PolyBucket.Database.Entities.Users;

public class UserFollow : Auditable
{
    public Guid FollowerUserId { get; set; }
    public virtual User Follower { get; set; } = null!;
    public Guid FollowedUserId { get; set; }
    public virtual User Followed { get; set; } = null!;
}
