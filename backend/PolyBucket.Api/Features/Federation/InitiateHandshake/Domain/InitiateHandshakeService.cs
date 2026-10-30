using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PolyBucket.Api.Features.Federation.InitiateHandshake.Http;
using PolyBucket.Api.Features.Federation.InitiateHandshake.Repository;

namespace PolyBucket.Api.Features.Federation.InitiateHandshake.Domain;

public interface IInitiateHandshakeService
{
    Task<ActionResult<InitiateHandshakeResponse>> InitiateHandshakeAsync(InitiateHandshakeRequest request, CancellationToken cancellationToken = default);
}

public class InitiateHandshakeService(
    IInitiateHandshakeRepository repository,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor) : IInitiateHandshakeService
{
    public async Task<ActionResult<InitiateHandshakeResponse>> InitiateHandshakeAsync(
        InitiateHandshakeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.InitiatorUrl))
        {
            return new BadRequestObjectResult("Initiator URL is required");
        }

        if (string.IsNullOrWhiteSpace(request.InitiatorName))
        {
            return new BadRequestObjectResult("Initiator name is required");
        }

        if (!Uri.TryCreate(request.InitiatorUrl, UriKind.Absolute, out var initiatorUri) ||
            initiatorUri.Scheme != Uri.UriSchemeHttps)
        {
            return new BadRequestObjectResult("Initiator URL must be a valid HTTPS URL");
        }

        if (!await VerifyRemoteInstanceAsync(request.InitiatorUrl))
        {
            return new UnprocessableEntityObjectResult("Could not verify remote instance as a valid PolyBucket instance");
        }

        var handshake = new FederationHandshake
        {
            Id = Guid.NewGuid(),
            InitiatorUrl = request.InitiatorUrl.TrimEnd('/'),
            ResponderUrl = GetThisInstanceUrl(),
            Status = HandshakeStatus.Accepted,
            HandshakeToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedById = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        repository.Context.FederationHandshakes.Add(handshake);
        await repository.Context.SaveChangesAsync(cancellationToken);

        return new OkObjectResult(new InitiateHandshakeResponse
        {
            HandshakeId = handshake.Id,
            Status = "Accepted",
            ResponderUrl = handshake.ResponderUrl,
            ResponderName = configuration["Instance:Name"] ?? "PolyBucket Instance",
            HandshakeToken = handshake.HandshakeToken,
            ExpiresAt = handshake.ExpiresAt,
            Message = "Handshake accepted. Proceed to token exchange."
        });
    }

    private async Task<bool> VerifyRemoteInstanceAsync(string url)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            var healthUrl = $"{url.TrimEnd('/')}/api/health";
            var response = await httpClient.GetAsync(healthUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string GetThisInstanceUrl()
    {
        var configuredUrl = configuration["Instance:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredUrl))
        {
            return configuredUrl.TrimEnd('/');
        }

        var request = httpContextAccessor.HttpContext!.Request;
        return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
    }
}
