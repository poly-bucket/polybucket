using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Federation.GetCatalog.Http;
using PolyBucket.Api.Features.Federation.GetCatalog.Repository;
using PolyBucket.Api.Features.Federation.Services;

namespace PolyBucket.Api.Features.Federation.GetCatalog.Domain;

public interface IGetCatalogService
{
    Task<ActionResult<CatalogResponse>> GetCatalogAsync(int page, int pageSize, DateTime? since, string? authorizationHeader, CancellationToken cancellationToken = default);
}

public class GetCatalogService(
    IGetCatalogRepository repository,
    IFederationInstanceIdentity instanceIdentity) : IGetCatalogService
{
    public async Task<ActionResult<CatalogResponse>> GetCatalogAsync(int page, int pageSize, DateTime? since, string? authorizationHeader, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return new UnauthorizedObjectResult("Federation token required");

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var identity = await instanceIdentity.GetSnapshotAsync(cancellationToken);
        var (totalCount, models) = await repository.GetCatalogPageAsync(page, pageSize, since, cancellationToken);
        return new OkObjectResult(new CatalogResponse
        {
            InstanceId = identity.InstanceId,
            InstanceName = identity.InstanceName,
            InstanceDescription = identity.InstanceDescription,
            AdminContact = identity.AdminContact,
            BaseUrl = identity.BaseUrl,
            TotalModels = totalCount,
            Page = page,
            PageSize = pageSize,
            Models = models
        });
    }
}
