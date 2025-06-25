using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Features.Printers.Domain.Enums;

namespace PolyBucket.Api.Features.Filaments.Domain;

public class Filament : BaseEntity
{
    public required string Manufacturer { get; set; }

    public MaterialType Type { get; set; }

    public required string Color { get; set; }

    public required string Diameter { get; set; }
} 