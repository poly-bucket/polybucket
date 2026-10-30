using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Http;
using PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Repository;

namespace PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Domain;

public interface IUpdateFederatedInstanceService
{
    Task<ActionResult<UpdateFederatedInstanceResponse>> UpdateFederatedInstanceAsync(
        Guid id,
        UpdateFederatedInstanceRequest request,
        CancellationToken cancellationToken = default);
}

public class UpdateFederatedInstanceService(
    IUpdateFederatedInstanceRepository repository,
    IHttpContextAccessor httpContextAccessor) : IUpdateFederatedInstanceService
{
    public async Task<ActionResult<UpdateFederatedInstanceResponse>> UpdateFederatedInstanceAsync(
        Guid id,
        UpdateFederatedInstanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetFederatedInstanceAsync(id, cancellationToken);
        if (existing == null)
        {
            return new NotFoundObjectResult($"Federated instance with ID {id} not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            existing.Name = request.Name;
        }

        if (!string.IsNullOrWhiteSpace(request.BaseUrl))
        {
            if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out _))
            {
                return new BadRequestObjectResult("Invalid base URL format");
            }

            existing.BaseUrl = request.BaseUrl.TrimEnd('/');
        }

        if (request.Description != null)
        {
            existing.Description = request.Description;
        }

        if (request.PublicKey != null)
        {
            existing.PublicKey = request.PublicKey;
        }

        if (request.SharedSecret != null)
        {
            existing.SharedSecret = request.SharedSecret;
        }

        if (request.IsEnabled.HasValue)
        {
            existing.IsEnabled = request.IsEnabled.Value;
        }

        if (request.Status != null && Enum.TryParse<FederationStatus>(request.Status, out var status))
        {
            existing.Status = status;
        }

        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return new UnauthorizedObjectResult("User is not authenticated");
        }

        existing.UpdatedById = Guid.Parse(userIdClaim);
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await repository.UpdateFederatedInstanceAsync(existing, cancellationToken);
        return new OkObjectResult(new UpdateFederatedInstanceResponse
        {
            Id = updated.Id,
            Name = updated.Name,
            BaseUrl = updated.BaseUrl,
            Status = updated.Status.ToString(),
            Description = updated.Description,
            IsEnabled = updated.IsEnabled,
            LastSyncAt = updated.LastSyncAt,
            UpdatedAt = updated.UpdatedAt
        });
    }
}
