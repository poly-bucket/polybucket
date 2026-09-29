using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.MODERATION_EDIT_MODELS)]
public class ModeratorEditModelController(IModeratorEditModelService service) : ControllerBase
{
    private readonly IModeratorEditModelService _service = service;

    /// <summary>
    /// Allows moderators to edit model details and metadata.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Model), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Model>> EditModel(
        Guid id,
        [FromBody] ModeratorEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var moderatorId))
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            var updatedModel = await _service.EditModelAsync(
                id,
                moderatorId,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);
            return Ok(updatedModel);
        }
        catch (NotFoundException)
        {
            return NotFound("Model not found");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to update model: {ex.Message}");
        }
    }

    /// <summary>
    /// Get model details for moderation editing.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Model), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Model>> GetModelForModeration(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _service.GetModelForModerationAsync(id, cancellationToken);
        if (model == null)
        {
            return NotFound("Model not found");
        }

        return Ok(model);
    }
}
