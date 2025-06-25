using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class CreateModelVersionController : ControllerBase
    {
        [HttpPost("{id}/versions")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> CreateModelVersion(Guid id/*, [FromForm] ModelVersionUploadRequest request*/)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 