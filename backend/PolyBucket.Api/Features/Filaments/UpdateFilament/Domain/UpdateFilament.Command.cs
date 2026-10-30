using PolyBucket.Database.Entities.Printers.Enums;

namespace PolyBucket.Api.Features.Filaments.UpdateFilament.Domain;

public class UpdateFilamentCommand
{
    public Guid Id { get; set; }
    public required string Manufacturer { get; set; }
    public MaterialType Type { get; set; }
    public required string Color { get; set; }
    public required string Diameter { get; set; }
}
