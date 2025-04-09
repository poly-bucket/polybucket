using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.Files.Http
{
    [ApiController]
    [Route("api/files")]
    public class GetSupportedExtensionsByTypeController : ControllerBase
    {
        [HttpGet("extensions/by-type/{fileType}")]
        public ActionResult<IEnumerable<string>> GetSupportedExtensionsForType(/*FileType*/ string fileType)
        {
            // This will be re-implemented
            throw new System.NotImplementedException();
        }
    }
} 