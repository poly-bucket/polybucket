using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models;
using System;

namespace PolyBucket.Database.Entities.Models;

public class Like : Auditable
{
    public new Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Model Model { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public ModelReactionType Type { get; set; } = ModelReactionType.Like;
}
