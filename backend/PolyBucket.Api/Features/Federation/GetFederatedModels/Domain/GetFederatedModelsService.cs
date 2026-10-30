using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Federation.GetFederatedModels.Http;
using PolyBucket.Api.Features.Federation.GetFederatedModels.Repository;

namespace PolyBucket.Api.Features.Federation.GetFederatedModels.Domain;

public interface IGetFederatedModelsService
{
    Task<(ActionResult<List<FederatedModelDto>> Result, int TotalCount)> GetFederatedModelsAsync(
        string? instanceId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public class GetFederatedModelsService(IGetFederatedModelsRepository repository) : IGetFederatedModelsService
{
    public async Task<(ActionResult<List<FederatedModelDto>> Result, int TotalCount)> GetFederatedModelsAsync(
        string? instanceId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 50;
        }

        var query = repository.Context.Models
            .Where(m => m.IsFederated)
            .Include(m => m.Author)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(instanceId))
        {
            query = query.Where(m => m.RemoteInstanceId == instanceId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var models = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new FederatedModelDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                ThumbnailUrl = m.ThumbnailUrl,
                RemoteInstanceId = m.RemoteInstanceId,
                RemoteModelId = m.RemoteModelId,
                RemoteAuthorId = m.RemoteAuthorId,
                AuthorUsername = m.Author.Username,
                Downloads = m.Downloads,
                Likes = m.Likes,
                LastFederationSync = m.LastFederationSync,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return (new OkObjectResult(models), totalCount);
    }
}
