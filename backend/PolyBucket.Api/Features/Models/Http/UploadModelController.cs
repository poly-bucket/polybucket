using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class UploadModelController : ControllerBase
    {
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UploadModel(/*[FromForm] ModelUploadRequest request*/)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 