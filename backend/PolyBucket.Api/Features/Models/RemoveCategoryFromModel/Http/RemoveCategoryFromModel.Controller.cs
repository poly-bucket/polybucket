using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class RemoveCategoryFromModelController : ControllerBase
    {
        [HttpDelete("{id}/categories/{categoryId}")]
        public async Task<ActionResult> RemoveCategoryFromModel(Guid id, Guid categoryId)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
}
