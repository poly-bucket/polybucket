using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.DownloadModel.Repository;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DownloadModel.Repository;

public class DownloadModelRepositoryTests
{
    [Fact]
    public async Task GetBundleForDownloadAsync_WhenModelMissing_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("download_repo_empty_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);
        var sut = new DownloadModelRepository(ctx);

        var result = await sut.GetBundleForDownloadAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBundleForDownloadAsync_WhenModelExists_ReturnsBundleWithFiles()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("download_repo_data_" + Guid.NewGuid())
            .Options;
        var modelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        await using (var ctx = new PolyBucketDbContext(options))
        {
            var model = new Model
            {
                Id = modelId,
                Name = "M",
                Description = "D",
                Privacy = PrivacySettings.Public,
                AuthorId = authorId,
                Files = new List<ModelFile>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "f.stl",
                        Path = "key/1",
                        Size = 1,
                        MimeType = "model/stl"
                    }
                }
            };
            ctx.Add(model);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = new PolyBucketDbContext(options);
        var sut = new DownloadModelRepository(readCtx);

        var result = await sut.GetBundleForDownloadAsync(modelId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(modelId, result!.Id);
        Assert.Single(result.Files);
        Assert.Equal("f.stl", result.Files[0].Name);
    }
}
