using MediatR;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Queries;

public class GetAllFilamentsHandler(PolyBucketDbContext context) : IRequestHandler<GetAllFilamentsQuery, List<Filament>>
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<List<Filament>> Handle(GetAllFilamentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Filaments.ToListAsync(cancellationToken);
    }
} 