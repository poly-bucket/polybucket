using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.GetFederationHealth.Http;
using PolyBucket.Api.Features.Federation.GetFederationHealth.Repository;

namespace PolyBucket.Api.Features.Federation.GetFederationHealth.Domain;

public interface IGetFederationHealthService
{
    Task<ActionResult<FederationHealthResponse>> GetFederationHealthAsync(Guid instanceId, CancellationToken cancellationToken = default);
}

public class GetFederationHealthService(
    IGetFederationHealthRepository repository,
    IHttpClientFactory httpClientFactory) : IGetFederationHealthService
{
    public async Task<ActionResult<FederationHealthResponse>> GetFederationHealthAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        var instance = await repository.GetFederatedInstanceAsync(instanceId, cancellationToken);
        if (instance == null)
        {
            return new NotFoundObjectResult($"Federated instance with ID {instanceId} not found");
        }

        var healthStatus = await CheckInstanceHealthAsync(instance.BaseUrl);
        return new OkObjectResult(new FederationHealthResponse
        {
            InstanceId = instance.Id,
            InstanceName = instance.Name,
            BaseUrl = instance.BaseUrl,
            Status = instance.Status.ToString(),
            IsEnabled = instance.IsEnabled,
            IsReachable = healthStatus.IsReachable,
            ResponseTimeMs = healthStatus.ResponseTimeMs,
            LastSyncAt = instance.LastSyncAt,
            ErrorMessage = healthStatus.ErrorMessage,
            CheckedAt = DateTime.UtcNow
        });
    }

    private async Task<HealthStatus> CheckInstanceHealthAsync(string baseUrl)
    {
        var startTime = DateTime.UtcNow;
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            var healthUrl = $"{baseUrl.TrimEnd('/')}/api/health";
            var response = await httpClient.GetAsync(healthUrl);
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            return new HealthStatus
            {
                IsReachable = response.IsSuccessStatusCode,
                ResponseTimeMs = (int)responseTime,
                ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
            };
        }
        catch (HttpRequestException ex)
        {
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            return new HealthStatus
            {
                IsReachable = false,
                ResponseTimeMs = (int)responseTime,
                ErrorMessage = $"Connection failed: {ex.Message}"
            };
        }
        catch (TaskCanceledException)
        {
            return new HealthStatus
            {
                IsReachable = false,
                ResponseTimeMs = 10000,
                ErrorMessage = "Request timeout"
            };
        }
    }
}
