using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Manages importing of models from federated instances
    /// </summary>
    [ApiController]
    [Route("api/federation/models")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class ImportFederatedModelController(IFederationImportService importService) : ControllerBase
    {
        private readonly IFederationImportService _importService = importService;

        /// <summary>
        /// Import a model from a federated instance
        /// </summary>
        /// <param name="request">The import request containing instance and model identifiers</param>
        /// <remarks>
        /// Imports a model from a remote federated instance by:
        /// 1. Fetching model metadata from the remote instance
        /// 2. Creating or updating the federated user (author) locally
        /// 3. Downloading all model files to local storage
        /// 4. Creating a local Model record with federation metadata
        /// 
        /// The imported model will be marked as federated and maintain attribution to the original author and instance.
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="201">Model imported successfully</response>
        /// <response code="400">Bad request - validation failed or invalid parameters</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        /// <response code="404">Federated instance or remote model not found</response>
        /// <response code="500">Internal server error during import process</response>
        [HttpPost("import")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(ImportFederatedModelResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ImportFederatedModelResponse>> ImportFederatedModel(
            [FromBody] ImportFederatedModelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InstanceId))
            {
                return BadRequest("Instance ID is required");
            }

            if (string.IsNullOrWhiteSpace(request.RemoteModelId))
            {
                return BadRequest("Remote model ID is required");
            }

            try
            {
                var model = await _importService.ImportModelAsync(request.InstanceId, request.RemoteModelId);

                return CreatedAtAction(
                    nameof(GetFederatedModelsController.GetFederatedModels),
                    "GetFederatedModels",
                    new { },
                    new ImportFederatedModelResponse
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Description = model.Description,
                        RemoteInstanceId = model.RemoteInstanceId,
                        RemoteModelId = model.RemoteModelId,
                        IsFederated = model.IsFederated,
                        LastFederationSync = model.LastFederationSync,
                        CreatedAt = model.CreatedAt
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to import model: {ex.Message}");
            }
        }
    }

    public class ImportFederatedModelRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string RemoteModelId { get; set; } = string.Empty;
    }

    public class ImportFederatedModelResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RemoteInstanceId { get; set; }
        public string? RemoteModelId { get; set; }
        public bool IsFederated { get; set; }
        public DateTime? LastFederationSync { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

