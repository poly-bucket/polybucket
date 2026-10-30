using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.GetCatalogChanges.Http;
using PolyBucket.Api.Features.Federation.GetCatalogChanges.Repository;

namespace PolyBucket.Api.Features.Federation.GetCatalogChanges.Domain;

public interface IGetCatalogChangesService
{
    Task<ActionResult<CatalogChangesResponse>> GetChangesAsync(DateTime since, string? authorizationHeader, CancellationToken cancellationToken = default);
}

public class GetCatalogChangesService(IGetCatalogChangesRepository repository) : IGetCatalogChangesService
{
    public async Task<ActionResult<CatalogChangesResponse>> GetChangesAsync(DateTime since, string? authorizationHeader, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return new UnauthorizedObjectResult("Federation token required");

        var response = await repository.GetChangesAsync(since, cancellationToken);
        return new OkObjectResult(response);
    }
}
