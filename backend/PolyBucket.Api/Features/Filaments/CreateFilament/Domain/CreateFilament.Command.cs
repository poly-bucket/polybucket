using PolyBucket.Database.Entities.Printers.Enums;

namespace PolyBucket.Api.Features.Filaments.CreateFilament.Domain;

public class CreateFilamentCommand
{
    public required string Manufacturer { get; set; }
    public MaterialType Type { get; set; }
    public required string Color { get; set; }
    public required string Diameter { get; set; }
} 