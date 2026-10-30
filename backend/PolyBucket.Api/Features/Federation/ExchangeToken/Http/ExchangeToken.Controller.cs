using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Federation.ExchangeToken.Domain;
using PolyBucket.Api.Features.Federation.ExchangeToken.Http;

namespace PolyBucket.Api.Features.Federation.ExchangeToken.Http;

[ApiController]
    [Route("api/federation/handshake")]
    [AllowAnonymous] // Allow anonymous since tokens are being exchanged
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation Handshake")]
    public class ExchangeTokenController(IExchangeTokenService service) : ControllerBase
{
    [HttpPost("exchange-token")]
        [ProducesResponseType(typeof(ExchangeTokenResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ExchangeTokenResponse>> ExchangeToken([FromBody] ExchangeTokenRequest request, CancellationToken cancellationToken = default)
    {
        return await service.ExchangeTokenAsync(request, cancellationToken);
    }
}
