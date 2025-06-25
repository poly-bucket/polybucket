using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [ApiController]
    [Route("api/models")]
    public class DownloadModelController : ControllerBase
    {
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadModel(Guid id)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 