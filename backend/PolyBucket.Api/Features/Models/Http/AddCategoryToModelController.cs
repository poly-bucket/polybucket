using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class AddCategoryToModelController : ControllerBase
    {
        [HttpPost("{id}/categories/{categoryId}")]
        public async Task<ActionResult> AddCategoryToModel(Guid id, Guid categoryId)
        {
            // This will be re-implemented
            throw new NotImplementedException();
        }
    }
} 