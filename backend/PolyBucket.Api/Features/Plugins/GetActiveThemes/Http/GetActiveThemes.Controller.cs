using PolyBucket.Api.Features.Plugins.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetActiveThemes.Domain;

namespace PolyBucket.Api.Features.Plugins.GetActiveThemes.Http;

[ApiController]
[Route("api/plugins/themes")]
[Authorize]
public class GetActiveThemesController(IGetActiveThemesService service) : ControllerBase
{
    [HttpGet("active")]
    public ActionResult<List<ActiveTheme>> GetActiveThemes() => Ok(service.GetActiveThemes());
}
