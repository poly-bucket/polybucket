using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Printers.Queries
{
    [ApiController]
    [Route("api/printers")]
    [Authorize]
    public class GetPrintersController : ControllerBase
    {
        private readonly GetPrintersQueryHandler _handler;

        public GetPrintersController(GetPrintersQueryHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrinters(CancellationToken cancellationToken)
        {
            var response = await _handler.Handle(new GetPrintersQuery(), cancellationToken);
            return Ok(response);
        }
    }
} 