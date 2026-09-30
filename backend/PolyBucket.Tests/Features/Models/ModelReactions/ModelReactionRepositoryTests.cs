using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.ModelReactions.Domain;
using PolyBucket.Api.Features.Models.ModelReactions.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.ModelReactions;

public class ModelReactionRepositoryTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly ModelReactionRepository _repository;

    public ModelReactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new PolyBucketDbContext(options);
        _repository = new ModelReactionRepository(_context, TimeProvider.System);
    }

    [Fact(DisplayName = "When model likes are enabled in settings, reactions are reported as enabled.")]
    public async Task IsReactionsEnabledAsync_WhenEnabled_ReturnsTrue()
    {
        _context.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            EnableModelLikes = true
        });
        await _context.SaveChangesAsync();

        var result = await _repository.IsReactionsEnabledAsync(CancellationToken.None);

        result.ShouldBeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
