using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Repository;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings.DeleteAllFileSettings;

public class DeleteAllFileSettingsRepositoryTests
{
    [Fact(DisplayName = "When deleting all file settings and there are none, the repository returns zero.")]
    public async Task DeleteAllFileSettingsAndReturnCountAsync_WhenNone_ReturnsZero()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("delete_all_file_settings_empty_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);
        var sut = new DeleteAllFileSettingsRepository(ctx);

        var count = await sut.DeleteAllFileSettingsAndReturnCountAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0, count);
    }

    [Fact(DisplayName = "When file type settings exist, the repository soft-deletes them.")]
    public async Task DeleteAllFileSettingsAndReturnCountAsync_WhenFileTypesExist_SoftDeletes()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("delete_all_file_settings_seeded_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);

        var fileType = new FileTypeSettings
        {
            Id = Guid.NewGuid(),
            FileExtension = ".stl",
            DisplayName = "STL",
            Category = "3D"
        };
        ctx.FileTypeSettings.Add(fileType);
        await ctx.SaveChangesAsync();

        var sut = new DeleteAllFileSettingsRepository(ctx);
        var deletedBy = Guid.NewGuid();

        var count = await sut.DeleteAllFileSettingsAndReturnCountAsync(deletedBy, CancellationToken.None);

        Assert.Equal(1, count);
        var reloaded = await ctx.FileTypeSettings.FindAsync(fileType.Id);
        Assert.NotNull(reloaded?.DeletedAt);
        Assert.Equal(deletedBy, reloaded.DeletedById);
    }
}
