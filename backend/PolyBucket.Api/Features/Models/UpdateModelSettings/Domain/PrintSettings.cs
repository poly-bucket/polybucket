using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Models.UpdateModelSettings.Domain
{
    public class PrintSettings : Auditable
    {
        public new Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
        public string LayerHeight { get; set; } = string.Empty;
        public string InfillPercentage { get; set; } = string.Empty;
        public string PrintSpeed { get; set; } = string.Empty;
        public string BedTemperature { get; set; } = string.Empty;
        public string ExtruderTemperature { get; set; } = string.Empty;
        public string SupportType { get; set; } = string.Empty;
        public string Raft { get; set; } = string.Empty;
        public string Brim { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
