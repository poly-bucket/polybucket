using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Categories.DeleteAllCategories.Repository;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Categories.DeleteAllCategories;

public class DeleteAllCategoriesRepositoryTests
{
    [Fact(DisplayName = "When deleting all categories and there are none, the repository returns zero.")]
    public async Task DeleteAllCategoriesAndReturnCountAsync_WhenNoCategories_ReturnsZero()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("delete_all_categories_empty_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);
        var sut = new DeleteAllCategoriesRepository(ctx);

        var count = await sut.DeleteAllCategoriesAndReturnCountAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0, count);
    }

    [Fact(DisplayName = "When categories exist, the repository soft-deletes them and clears model links.")]
    public async Task DeleteAllCategoriesAndReturnCountAsync_WhenCategoriesExist_SoftDeletesAndClearsLinks()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("delete_all_categories_seeded_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);

        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "M",
            Description = "D",
            AuthorId = Guid.NewGuid(),
            Categories = new List<Category> { category }
        };
        ctx.Categories.Add(category);
        ctx.Models.Add(model);
        await ctx.SaveChangesAsync();

        var sut = new DeleteAllCategoriesRepository(ctx);
        var deletedBy = Guid.NewGuid();

        var count = await sut.DeleteAllCategoriesAndReturnCountAsync(deletedBy, CancellationToken.None);

        Assert.Equal(1, count);
        var reloadedCategory = await ctx.Categories.FindAsync(category.Id);
        Assert.NotNull(reloadedCategory?.DeletedAt);
        Assert.Equal(deletedBy, reloadedCategory.DeletedById);
        var reloadedModel = await ctx.Models.Include(m => m.Categories).FirstAsync(m => m.Id == model.Id);
        Assert.Empty(reloadedModel.Categories);
    }
}
