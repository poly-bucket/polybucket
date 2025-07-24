using MediatR;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Queries;

public class GetFilamentByIdHandler(PolyBucketDbContext context) : IRequestHandler<GetFilamentByIdQuery, Filament?>
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<Filament?> Handle(GetFilamentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Filaments.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
    }
} 