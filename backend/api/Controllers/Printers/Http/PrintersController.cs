using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Controllers.Printers.Domain;

namespace api.Controllers.Printers.Http;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrintersController : ControllerBase
{
    private readonly IGetPrintersService _getPrintersService;

    public PrintersController(IGetPrintersService getPrintersService)
    {
        _getPrintersService = getPrintersService;
    }

    [HttpGet]
    public async Task<ActionResult<GetPrintersResponse>> GetPrinters()
    {
        var response = await _getPrintersService.ExecuteAsync();
        return Ok(response);
    }
} 