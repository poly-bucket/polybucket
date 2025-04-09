using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [ApiController]
    [Route("api/models")]
    public class SearchModelsController : ControllerBase
    {
        [HttpGet("search")]
        public async Task<ActionResult> SearchModels([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 