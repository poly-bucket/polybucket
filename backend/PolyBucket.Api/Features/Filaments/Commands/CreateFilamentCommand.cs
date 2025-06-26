using MediatR;
using PolyBucket.Api.Features.Filaments.Domain;
using PolyBucket.Api.Features.Printers.Domain.Enums;

namespace PolyBucket.Api.Features.Filaments.Commands;

public class CreateFilamentCommand : IRequest<Filament>
{
    public required string Manufacturer { get; set; }
    public MaterialType Type { get; set; }
    public required string Color { get; set; }
    public required string Diameter { get; set; }
} 