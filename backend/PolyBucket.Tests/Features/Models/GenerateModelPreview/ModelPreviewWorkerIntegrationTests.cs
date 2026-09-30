using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GenerateModelPreview;

[Collection("TestCollection")]
public class ModelPreviewWorkerIntegrationTests : BaseIntegrationTest
{
    public ModelPreviewWorkerIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task<Guid> SeedModelWithStlAsync(Guid authorId)
    {
        var model = await ModelFactory.CreateTestModel("Preview target", "desc", authorId);
        DbContext.Set<ModelFile>().Add(new ModelFile
        {
            Id = Guid.NewGuid(),
            ModelId = model.Id,
            Name = "part.stl",
            Path = $"models/{model.Id}/part.stl",
            Size = 1024,
            MimeType = "model/stl",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
        return model.Id;
    }

    private async Task EnqueueAsync(Guid modelId)
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IModelPreviewQueue>().EnqueueAsync(modelId, "thumbnail", forceRegenerate: false, default);
    }

    private async Task<int> ProcessAsync()
    {
        using var scope = Factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IModelPreviewProcessor>().ProcessPendingAsync();
    }

    private async Task<ModelPreview> ReadPreviewAsync(Guid modelId)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        return await context.ModelPreviews.AsNoTracking().SingleAsync(p => p.ModelId == modelId && p.Size == "thumbnail");
    }

    [Fact(DisplayName = "When a pending preview is processed, the worker renders the model's STL and marks it completed.")]
    public async Task Pending_IsCompleted()
    {
        // Arrange
        await ResetStateAsync();
        Factory.PreviewGenerator.Reset();
        var author = await CreateTestUser();
        var modelId = await SeedModelWithStlAsync(author.Id);
        await EnqueueAsync(modelId);

        // Act
        var processed = await ProcessAsync();

        // Assert
        processed.ShouldBe(1);
        var preview = await ReadPreviewAsync(modelId);
        preview.Status.ShouldBe(PreviewStatus.Completed);
        preview.Attempts.ShouldBe(1);
        preview.StorageKey.ShouldBe($"previews/{modelId}/thumbnail.png");
        preview.LockedUntil.ShouldBeNull();
        Factory.PreviewGenerator.Calls.Single().FileName.ShouldBe("part.stl");
    }

    [Fact(DisplayName = "When rendering fails once, the preview waits for its retry time and then completes on the next attempt.")]
    public async Task Failed_IsRetriedThenCompleted()
    {
        // Arrange
        await ResetStateAsync();
        Factory.PreviewGenerator.Reset();
        Factory.PreviewGenerator.FailNext(1);
        var author = await CreateTestUser();
        var modelId = await SeedModelWithStlAsync(author.Id);
        await EnqueueAsync(modelId);

        // Act
        await ProcessAsync();
        var afterFailure = await ReadPreviewAsync(modelId);
        var processedBeforeRetryDue = await ProcessAsync();
        await DbContext.ModelPreviews
            .Where(p => p.ModelId == modelId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.NextAttemptAt, DateTime.UtcNow.AddSeconds(-1)));
        await ProcessAsync();
        var afterRetry = await ReadPreviewAsync(modelId);

        // Assert
        afterFailure.Status.ShouldBe(PreviewStatus.Pending);
        afterFailure.Attempts.ShouldBe(1);
        afterFailure.ErrorMessage.ShouldBe("Renderer unavailable");
        afterFailure.NextAttemptAt.ShouldNotBeNull();
        processedBeforeRetryDue.ShouldBe(0);
        afterRetry.Status.ShouldBe(PreviewStatus.Completed);
        afterRetry.Attempts.ShouldBe(2);
        afterRetry.ErrorMessage.ShouldBeNull();
    }

    [Fact(DisplayName = "When rendering keeps failing, the preview is marked failed after the attempt limit.")]
    public async Task RepeatedFailure_IsDeadLettered()
    {
        // Arrange
        await ResetStateAsync();
        Factory.PreviewGenerator.Reset();
        Factory.PreviewGenerator.FailNext(10);
        var author = await CreateTestUser();
        var modelId = await SeedModelWithStlAsync(author.Id);
        await EnqueueAsync(modelId);

        // Act
        for (var i = 0; i < 3; i++)
        {
            await DbContext.ModelPreviews
                .Where(p => p.ModelId == modelId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.NextAttemptAt, DateTime.UtcNow.AddSeconds(-1)));
            await ProcessAsync();
        }
        var preview = await ReadPreviewAsync(modelId);

        // Assert
        preview.Status.ShouldBe(PreviewStatus.Failed);
        preview.Attempts.ShouldBe(3);
        Factory.PreviewGenerator.Calls.Count.ShouldBe(3);
    }

    [Fact(DisplayName = "When two workers process at once, a preview is claimed and rendered only once.")]
    public async Task ConcurrentWorkers_ClaimOnce()
    {
        // Arrange
        await ResetStateAsync();
        Factory.PreviewGenerator.Reset();
        var author = await CreateTestUser();
        var modelId = await SeedModelWithStlAsync(author.Id);
        await EnqueueAsync(modelId);

        // Act
        var results = await Task.WhenAll(ProcessAsync(), ProcessAsync());

        // Assert
        results.Sum().ShouldBe(1);
        Factory.PreviewGenerator.Calls.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "When a non-owner requests a preview through the API, the request is forbidden.")]
    public async Task Api_NonOwner_Forbidden()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateTestUser();
        var other = await CreateTestUser("preview-other@polybucket.test", "Password123!");
        var modelId = await SeedModelWithStlAsync(author.Id);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthToken(other.Email, "Password123!"));

        // Act
        var response = await Client.PostAsync($"/api/models/{modelId}/previews?forceRegenerate=true", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
