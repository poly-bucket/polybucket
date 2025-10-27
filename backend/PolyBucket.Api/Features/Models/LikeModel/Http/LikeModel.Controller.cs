using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.LikeModel.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class LikeModelController : ControllerBase
    {
        [HttpPost("{id}/like")]
        public async Task<ActionResult> LikeModel(Guid id)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
}
