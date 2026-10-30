using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PolyBucket.Api.Features.Federation.ExchangeToken.Http;
using PolyBucket.Api.Features.Federation.ExchangeToken.Repository;
using PolyBucket.Api.Features.Federation.Services;

namespace PolyBucket.Api.Features.Federation.ExchangeToken.Domain;

public interface IExchangeTokenService
{
    Task<ActionResult<ExchangeTokenResponse>> ExchangeTokenAsync(ExchangeTokenRequest request, CancellationToken cancellationToken = default);
}

public class ExchangeTokenService(
    IExchangeTokenRepository repository,
    IFederationTokenService tokenService,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor) : IExchangeTokenService
{
    public async Task<ActionResult<ExchangeTokenResponse>> ExchangeTokenAsync(ExchangeTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HandshakeId == Guid.Empty)
        {
            return new BadRequestObjectResult("Handshake ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.HandshakeToken))
        {
            return new BadRequestObjectResult("Handshake token is required");
        }

        var handshake = await repository.Context.FederationHandshakes
            .FirstOrDefaultAsync(h => h.Id == request.HandshakeId, cancellationToken);

        if (handshake == null)
        {
            return new NotFoundObjectResult("Handshake not found");
        }

        if (handshake.HandshakeToken != request.HandshakeToken)
        {
            return new UnauthorizedObjectResult("Invalid handshake token");
        }

        if (handshake.ExpiresAt < DateTime.UtcNow)
        {
            handshake.Status = HandshakeStatus.Expired;
            await repository.Context.SaveChangesAsync(cancellationToken);
            return new UnauthorizedObjectResult("Handshake has expired");
        }

        var instance = await GetOrCreateFederatedInstanceAsync(request, handshake, cancellationToken);
        var thisInstanceId = instance.Id;
        var thisInstanceUrl = GetThisInstanceUrl();

        var federationToken = await tokenService.GenerateTokenAsync(
            thisInstanceId,
            thisInstanceId,
            thisInstanceUrl,
            expirationDays: 90);

        instance.OurToken = await tokenService.EncryptTokenAsync(federationToken);
        instance.OurTokenExpiry = DateTime.UtcNow.AddDays(90);

        if (!string.IsNullOrWhiteSpace(request.TheirToken))
        {
            var isValid = await tokenService.ValidateTokenAsync(request.TheirToken, thisInstanceId);
            if (!isValid)
            {
                return new BadRequestObjectResult("Provided token is invalid");
            }

            instance.TheirToken = await tokenService.EncryptTokenAsync(request.TheirToken);
            instance.TheirTokenExpiry = await tokenService.GetTokenExpirationAsync(request.TheirToken);
            handshake.Status = HandshakeStatus.TokenExchanged;
            instance.Status = FederationStatus.Active;
        }
        else
        {
            handshake.Status = HandshakeStatus.Accepted;
        }

        await repository.Context.SaveChangesAsync(cancellationToken);

        return new OkObjectResult(new ExchangeTokenResponse
        {
            OurToken = federationToken,
            TokenExpiry = instance.OurTokenExpiry!.Value,
            Status = handshake.Status.ToString(),
            InstanceId = instance.Id,
            Message = handshake.Status == HandshakeStatus.TokenExchanged
                ? "Token exchange complete. Proceed to catalog sharing."
                : "Token sent. Awaiting remote instance token."
        });
    }

    private async Task<FederatedInstance> GetOrCreateFederatedInstanceAsync(
        ExchangeTokenRequest request,
        FederationHandshake handshake,
        CancellationToken cancellationToken)
    {
        var thisInstanceUrl = GetThisInstanceUrl();
        var isInitiator = handshake.InitiatorUrl != thisInstanceUrl;
        var remoteUrl = isInitiator ? handshake.ResponderUrl : handshake.InitiatorUrl;
        var remoteName = request.InstanceName ?? "Remote Instance";

        var instance = await repository.Context.FederatedInstances
            .FirstOrDefaultAsync(i => i.BaseUrl == remoteUrl, cancellationToken);

        if (instance == null)
        {
            instance = new FederatedInstance
            {
                Id = Guid.NewGuid(),
                Name = remoteName,
                BaseUrl = remoteUrl,
                Status = FederationStatus.Pending,
                IsEnabled = true,
                CreatedById = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            repository.Context.FederatedInstances.Add(instance);

            if (isInitiator)
            {
                handshake.ResponderInstanceId = instance.Id;
            }
            else
            {
                handshake.InitiatorInstanceId = instance.Id;
            }
        }

        return instance;
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
