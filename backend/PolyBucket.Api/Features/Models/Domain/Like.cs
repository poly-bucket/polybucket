using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class Like : Auditable
    {
        public new Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required User User { get; set; }
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
    }
} 