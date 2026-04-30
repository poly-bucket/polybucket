using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.DeleteAllModels.Repository;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteAllModels.Repository;

public class DeleteAllModelsRepositoryTests
{
    [Fact(DisplayName = "When deleting all models and there are none, the delete all models repository returns zero.")]
    public async Task DeleteAllModelsAndReturnCountAsync_WhenNoModels_ReturnsZero()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("delete_all_empty_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);
        var sut = new DeleteAllModelsRepository(ctx);

        var count = await sut.DeleteAllModelsAndReturnCountAsync(CancellationToken.None);

        Assert.Equal(0, count);
    }
}
