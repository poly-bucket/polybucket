using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class UpdateModelController : ControllerBase
    {
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateModel(Guid id/*, [FromForm] ModelUpdateRequest request*/)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 