using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Printers.Enums;
using System.Collections.Generic;

namespace PolyBucket.Database.Entities.Printers
{
    public enum PrinterType
    {
        FDM,
        SLA,
        DLP,
        SLS
    }

    public class Printer : Auditable
    {
        public new Guid Id { get; set; }
        public string Manufacturer { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int BuildVolumeX { get; set; }
        public int BuildVolumeY { get; set; }
        public int BuildVolumeZ { get; set; }
        public int MaxBedTemp { get; set; }
        public int MaxNozzleTemp { get; set; }
        public bool HasHeatedBed { get; set; }
        public bool HasHeatedChamber { get; set; }
        public bool HasEnclosure { get; set; }
        public decimal DefaultNozzleDiameter { get; set; } = 0.4m;
        public int ExtruderCount { get; set; } = 1;
        public ExtruderType ExtruderType { get; set; }
        public List<ConnectivityType> Connectivity { get; set; } = new();
        public List<MaterialType> SupportedMaterials { get; set; } = new();
        public decimal? PriceUSD { get; set; }
        public int ReleaseYear { get; set; }
        public string? Description { get; set; }
        public PrinterType Type { get; set; }
        public bool HasAutoLeveling { get; set; }
        public bool HasFilamentSensor { get; set; }
        public bool HasPowerLossContinue { get; set; }
        public decimal MaxPrintSpeed { get; set; } // mm/s
        public string? WebsiteUrl { get; set; }
    }
} 