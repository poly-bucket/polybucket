using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.Files.Http
{
    [ApiController]
    [Route("api/files")]
    public class GetSupportedExtensionsController : ControllerBase
    {
        [HttpGet("extensions")]
        public ActionResult<IEnumerable<string>> GetSupportedExtensions()
        {
            // This will be re-implemented
            throw new System.NotImplementedException();
        }
    }
} 