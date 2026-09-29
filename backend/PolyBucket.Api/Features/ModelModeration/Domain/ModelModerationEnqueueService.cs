using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ModelModeration.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Domain;

public class ModelModerationEnqueueService(
    IModelModerationPolicy policy,
    IModelModerationSettingsProvider settingsProvider,
    IModelModerationRecordRepository recordRepository) : IModelModerationEnqueueService
{
    private readonly IModelModerationPolicy _policy = policy;
    private readonly IModelModerationSettingsProvider _settingsProvider = settingsProvider;
    private readonly IModelModerationRecordRepository _recordRepository = recordRepository;

    public async Task ApplyModerationStateForNewModelAsync(Model model, Guid authorId, CancellationToken cancellationToken = default)
    {
        await ApplyAsync(model, authorId, cancellationToken);
    }

    public async Task ApplyModerationStateForPublicVisibilityChangeAsync(Model model, Guid authorId, CancellationToken cancellationToken = default)
    {
        var existing = await _recordRepository.GetByModelIdAsync(model.Id, cancellationToken);
        if (existing?.Status == ModelModerationStatus.Approved)
        {
            model.IsPublic = model.Privacy != PrivacySettings.Private;
            await _recordRepository.UpdateModelAsync(model, cancellationToken);
            await _recordRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        if (existing?.Status == ModelModerationStatus.Pending)
        {
            model.IsPublic = false;
            await _recordRepository.UpdateModelAsync(model, cancellationToken);
            await _recordRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        if (model.Privacy == PrivacySettings.Private)
        {
            return;
        }

        await ApplyAsync(model, authorId, cancellationToken);
    }

    private async Task ApplyAsync(Model model, Guid authorId, CancellationToken cancellationToken)
    {
        if (model.Privacy == PrivacySettings.Private)
        {
            return;
        }

        var settings = await _settingsProvider.GetSnapshotAsync(cancellationToken);
        var emailVerified = await _recordRepository.IsAuthorEmailVerifiedAsync(authorId, cancellationToken);
        var autoApprove = _policy.ShouldAutoApprove(settings, emailVerified);

        if (autoApprove)
        {
            model.IsPublic = model.Privacy != PrivacySettings.Private;
            await _recordRepository.UpdateModelAsync(model, cancellationToken);
            await _recordRepository.UpsertRecordAsync(new ModelModerationRecord
            {
                Id = Guid.NewGuid(),
                ModelId = model.Id,
                Status = ModelModerationStatus.Approved,
                SubmittedAt = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            model.IsPublic = false;
            await _recordRepository.UpdateModelAsync(model, cancellationToken);
            await _recordRepository.UpsertRecordAsync(new ModelModerationRecord
            {
                Id = Guid.NewGuid(),
                ModelId = model.Id,
                Status = ModelModerationStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _recordRepository.SaveChangesAsync(cancellationToken);
    }
}
