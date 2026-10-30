using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Federation.GetCatalog.Http;
using PolyBucket.Api.Features.Federation.GetCatalog.Repository;

namespace PolyBucket.Api.Features.Federation.GetCatalog.Domain;

public interface IGetCatalogService
{
    Task<ActionResult<CatalogResponse>> GetCatalogAsync(int page, int pageSize, DateTime? since, string? authorizationHeader, CancellationToken cancellationToken = default);
}

public class GetCatalogService(IGetCatalogRepository repository) : IGetCatalogService
{
    public async Task<ActionResult<CatalogResponse>> GetCatalogAsync(int page, int pageSize, DateTime? since, string? authorizationHeader, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return new UnauthorizedObjectResult("Federation token required");

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var (totalCount, models) = await repository.GetCatalogPageAsync(page, pageSize, since, cancellationToken);
        return new OkObjectResult(new CatalogResponse
        {
            InstanceId = Guid.NewGuid(),
            InstanceName = "PolyBucket Instance",
            TotalModels = totalCount,
            Page = page,
            PageSize = pageSize,
            Models = models
        });
    }
}
