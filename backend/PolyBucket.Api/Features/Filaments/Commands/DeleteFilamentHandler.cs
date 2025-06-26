using MediatR;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Filaments.Commands;

public class DeleteFilamentHandler : IRequestHandler<DeleteFilamentCommand, bool>
{
    private readonly PolyBucketDbContext _context;

    public DeleteFilamentHandler(PolyBucketDbContext context)
    {
        _context = context;
    }

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