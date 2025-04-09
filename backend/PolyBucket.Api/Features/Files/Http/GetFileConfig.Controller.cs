using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.Files.Http
{
    [ApiController]
    [Route("api/files")]
    public class GetFileConfigController : ControllerBase
    {
        [HttpGet("config")]
        public ActionResult GetFileConfig()
        {
            // This will be re-implemented
            throw new System.NotImplementedException();
        }
    }
} 