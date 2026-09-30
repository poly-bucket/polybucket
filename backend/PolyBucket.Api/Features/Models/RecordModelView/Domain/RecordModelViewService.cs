using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.RecordModelView.Repository;

namespace PolyBucket.Api.Features.Models.RecordModelView.Domain;

public class RecordModelViewService(
    IRecordModelViewRepository repository,
    IPermissionService permissionService,
    TimeProvider timeProvider) : IRecordModelViewService
{
    private static readonly TimeSpan DedupWindow = TimeSpan.FromHours(24);

    public async Task<RecordModelViewOutcome> RecordViewAsync(
        Guid modelId,
        ClaimsPrincipal user,
        string viewerKey,
        CancellationToken cancellationToken = default)
    {
        var model = await repository.GetModelForViewAsync(modelId, cancellationToken);
        if (model == null)
        {
            return RecordModelViewOutcome.NotFound();
        }

        Guid? currentUserId = null;
        if (ModelEngagementViewerKey.TryResolveUserId(user, out var userId))
        {
            currentUserId = userId;
            if (model.AuthorId == userId)
            {
                return RecordModelViewOutcome.Ok(model.Views, counted: false);
            }
        }

        if (!await CanUserAccessModelAsync(model, currentUserId, cancellationToken))
        {
            return RecordModelViewOutcome.Forbid();
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var threshold = nowUtc - DedupWindow;
        var (views, counted) = await repository.TryRecordViewAsync(
            modelId,
            viewerKey,
            nowUtc,
            threshold,
            cancellationToken);

        return RecordModelViewOutcome.Ok(views, counted);
    }

    private async Task<bool> CanUserAccessModelAsync(
        Model model,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        if (model.Privacy == PrivacySettings.Public && model.IsPublic)
        {
            return true;
        }

        if (currentUserId is not { } userId)
        {
            return false;
        }

        if (model.AuthorId == userId)
        {
            return true;
        }

        var isAdmin = await permissionService.IsAdminAsync(userId);
        var userRole = await permissionService.GetUserRoleAsync(userId);
        var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;

        if (isAdmin || isModerator)
        {
            return true;
        }

        if (model.Privacy == PrivacySettings.Private)
        {
            return false;
        }

        if (model.Privacy == PrivacySettings.Unlisted)
        {
            return true;
        }

        return false;
    }
}
