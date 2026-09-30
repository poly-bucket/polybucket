using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.Models.GetModelVersions.Repository;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Domain;

public class GetModelVersionsService(
    IGetModelVersionsRepository repository,
    IPermissionService permissionService,
    IStorageService storageService) : IGetModelVersionsService
{
    private static readonly TimeSpan PresignedUrlLifetime = TimeSpan.FromHours(1);

    public async Task<GetModelVersionsResponse?> GetModelVersionsAsync(Guid modelId, Guid? viewerId, CancellationToken cancellationToken)
    {
        var access = await repository.GetAccessInfoAsync(modelId, cancellationToken);
        if (access == null || !await CanViewAsync(access, viewerId))
        {
            return null;
        }

        var versions = await repository.GetVersionsAsync(modelId, cancellationToken);
        foreach (var version in versions)
        {
            if (!string.IsNullOrEmpty(version.FileUrl))
            {
                version.FileUrl = await storageService.GetPresignedUrlAsync(version.FileUrl, PresignedUrlLifetime, cancellationToken);
            }

            if (!string.IsNullOrEmpty(version.ThumbnailUrl))
            {
                version.ThumbnailUrl = await storageService.GetPresignedUrlAsync(version.ThumbnailUrl, PresignedUrlLifetime, cancellationToken);
            }

            foreach (var file in version.Files.Where(f => !string.IsNullOrEmpty(f.Path)))
            {
                file.Path = await storageService.GetPresignedUrlAsync(file.Path, PresignedUrlLifetime, cancellationToken);
            }
        }

        return new GetModelVersionsResponse
        {
            ModelId = modelId,
            Versions = versions.Select(ModelDtoMapper.ToVersionDto).ToList()
        };
    }

    private async Task<bool> CanViewAsync(ModelVersionsAccessInfo access, Guid? viewerId)
    {
        if (access.Privacy == PrivacySettings.Public && access.IsPublic && access.PassedModeration)
        {
            return true;
        }

        if (viewerId is not { } userId)
        {
            return false;
        }

        if (access.AuthorId == userId)
        {
            return true;
        }

        if (await permissionService.IsAdminAsync(userId))
        {
            return true;
        }

        var role = await permissionService.GetUserRoleAsync(userId);
        if (role?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        return access.Privacy == PrivacySettings.Unlisted && access.PassedModeration;
    }
}
