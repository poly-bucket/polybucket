using MediatR;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Filaments.Domain;

namespace PolyBucket.Api.Features.Filaments.Handlers;

public class DeleteFilamentHandler(PolyBucketDbContext context) : IRequestHandler<DeleteFilamentCommand, bool>
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<bool> Handle(DeleteFilamentCommand request, CancellationToken cancellationToken)
    {
        var filament = await _context.Filaments.FindAsync(request.Id);

        if (filament == null)
        {
            return false;
        }

        _context.Filaments.Remove(filament);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
} 