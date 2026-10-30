using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Federation.InitiateHandshake.Domain;
using PolyBucket.Api.Features.Federation.InitiateHandshake.Http;

namespace PolyBucket.Api.Features.Federation.InitiateHandshake.Http;

[ApiController]
    [Route("api/federation/handshake")]
    [AllowAnonymous] // Allow anonymous since this is called by remote instances
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation Handshake")]
    public class InitiateHandshakeController(IInitiateHandshakeService service) : ControllerBase
{
    [HttpPost("initiate")]
        [ProducesResponseType(typeof(InitiateHandshakeResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<ActionResult<InitiateHandshakeResponse>> InitiateHandshake([FromBody] InitiateHandshakeRequest request, CancellationToken cancellationToken = default)
    {
        return await service.InitiateHandshakeAsync(request, cancellationToken);
    }
}
