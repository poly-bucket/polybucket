using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System;

namespace PolyBucket.Api.Features.Models.LikeModel.Domain
{
    public class Like : Auditable
    {
        public new Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
