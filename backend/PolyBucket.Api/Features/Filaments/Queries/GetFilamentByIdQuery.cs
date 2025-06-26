using MediatR;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Queries;

public class GetFilamentByIdQuery : IRequest<Filament?>
{
    public Guid Id { get; set; }
} 