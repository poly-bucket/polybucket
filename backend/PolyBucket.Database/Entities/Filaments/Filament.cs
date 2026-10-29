using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Printers.Enums;

namespace PolyBucket.Database.Entities.Filaments;

public class Filament : BaseEntity
{
    public required string Manufacturer { get; set; }

    public MaterialType Type { get; set; }

    public required string Color { get; set; }

    public required string Diameter { get; set; }
} 