using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class PrintSettings : Auditable
    {
        public new Guid Id { get; set; }
        public Model Model { get; set; } = null!;
        public float LayerHeight { get; set; }
        public int WallThickness { get; set; }
        public int InfillPercentage { get; set; }
        public bool SupportEnabled { get; set; }
        public string? Notes { get; set; }
    }
} 