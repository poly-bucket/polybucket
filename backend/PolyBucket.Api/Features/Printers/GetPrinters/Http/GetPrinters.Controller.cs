using Microsoft.AspNetCore.Authorization;
        using Microsoft.AspNetCore.Mvc;
        using PolyBucket.Api.Features.Printers.GetPrinters.Domain;

        namespace PolyBucket.Api.Features.Printers.GetPrinters.Http;

        [ApiController]
        [Route("api/printers")]
        [Authorize]
        public class GetPrintersController(IGetPrintersService service) : ControllerBase
        {
            /// <summary>
            /// Get the catalog of supported 3D printers.
            /// </summary>
            [HttpGet]
            [ProducesResponseType(typeof(GetPrintersResponseDto), StatusCodes.Status200OK)]
            public async Task<ActionResult<GetPrintersResponseDto>> GetPrinters(CancellationToken cancellationToken)
            {
                return Ok(await service.GetPrintersAsync(cancellationToken));
            }
        }
