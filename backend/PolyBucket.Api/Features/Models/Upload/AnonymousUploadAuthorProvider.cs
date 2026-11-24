using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Models.Upload;

public sealed class AnonymousUploadAuthorProvider(PolyBucketDbContext context) : IAnonymousUploadAuthorProvider
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<Guid> GetAuthorIdAsync(CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == AnonymousUploadAuthor.UserId, cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException(
                "Anonymous upload author is not provisioned. Run application seeding.");
        }

        return AnonymousUploadAuthor.UserId;
    }
}