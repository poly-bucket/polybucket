using MediatR;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Queries;

public class GetAllFilamentsQuery : IRequest<List<Filament>>
{
} 