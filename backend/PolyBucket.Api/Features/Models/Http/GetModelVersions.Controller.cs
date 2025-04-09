using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelVersionsController : ControllerBase
    {
        [HttpGet("{id}/versions")]
        public async Task<ActionResult> GetModelVersions(Guid id)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 