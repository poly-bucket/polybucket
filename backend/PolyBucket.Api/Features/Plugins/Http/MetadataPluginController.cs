using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Plugins.Http
{
    [ApiController]
    [Route("api/plugins/metadata")]
    [Authorize]
    public class MetadataPluginController : ControllerBase
    {
        private readonly MetadataPluginService _metadataService;
        private readonly ILogger<MetadataPluginController> _logger;

        public MetadataPluginController(
            MetadataPluginService metadataService,
            ILogger<MetadataPluginController> logger)
        {
            _metadataService = metadataService;
            _logger = logger;
        }

        /// <summary>
        /// Get all registered metadata plugins
        /// </summary>
        /// <returns>List of metadata plugins</returns>
        [HttpGet("plugins")]
        [ProducesResponseType(typeof(List<MetadataPluginInfo>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public ActionResult<List<MetadataPluginInfo>> GetMetadataPlugins()
        {
            try
            {
                var plugins = _metadataService.GetRegisteredMetadataPlugins();
                return Ok(plugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata plugins");
                return StatusCode(500, new { message = "Error retrieving metadata plugins" });
            }
        }

        /// <summary>
        /// Get metadata fields for a specific entity type
        /// </summary>
        /// <param name="entityType">Entity type (e.g., "model", "user", "collection")</param>
        /// <returns>List of metadata fields</returns>
        [HttpGet("fields/{entityType}")]
        [ProducesResponseType(typeof(List<MetadataField>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MetadataField>>> GetEntityFields(string entityType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                {
                    return BadRequest(new { message = "Entity type is required" });
                }

                var fields = await _metadataService.GetEntityFieldsAsync(entityType);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity fields for {EntityType}", entityType);
                return StatusCode(500, new { message = "Error retrieving entity fields" });
            }
        }

        /// <summary>
        /// Validate metadata values for an entity
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="entityId">Entity ID</param>
        /// <param name="request">Validation request</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate/{entityType}/{entityId}")]
        [ProducesResponseType(typeof(MetadataValidationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MetadataValidationResult>> ValidateMetadata(
            string entityType,
            string entityId,
            [FromBody] MetadataValidationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                {
                    return BadRequest(new { message = "Entity type is required" });
                }

                if (string.IsNullOrWhiteSpace(entityId))
                {
                    return BadRequest(new { message = "Entity ID is required" });
                }

                if (request.FieldValues == null)
                {
                    return BadRequest(new { message = "Field values are required" });
                }

                var result = await _metadataService.ValidateEntityMetadataAsync(entityType, entityId, request.FieldValues);
                
                if (result.IsValid)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating metadata for {EntityType}:{EntityId}", entityType, entityId);
                return StatusCode(500, new { message = "Error validating metadata" });
            }
        }

        /// <summary>
        /// Transform metadata values for an entity
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="request">Transform request</param>
        /// <returns>Transformed metadata values</returns>
        [HttpPost("transform/{entityType}")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Dictionary<string, object>>> TransformMetadata(
            string entityType,
            [FromBody] MetadataTransformRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                {
                    return BadRequest(new { message = "Entity type is required" });
                }

                if (request.FieldValues == null)
                {
                    return BadRequest(new { message = "Field values are required" });
                }

                var transformedValues = await _metadataService.TransformEntityMetadataAsync(entityType, request.FieldValues);
                return Ok(transformedValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming metadata for {EntityType}", entityType);
                return StatusCode(500, new { message = "Error transforming metadata" });
            }
        }

        /// <summary>
        /// Get default metadata values for an entity type
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>Default metadata values</returns>
        [HttpGet("defaults/{entityType}")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Dictionary<string, object>>> GetDefaultMetadata(string entityType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                {
                    return BadRequest(new { message = "Entity type is required" });
                }

                var defaultValues = await _metadataService.GetDefaultEntityMetadataAsync(entityType);
                return Ok(defaultValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default metadata for {EntityType}", entityType);
                return StatusCode(500, new { message = "Error retrieving default metadata" });
            }
        }

        /// <summary>
        /// Get metadata schema for an entity type
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>Metadata schema</returns>
        [HttpGet("schema/{entityType}")]
        [ProducesResponseType(typeof(MetadataSchema), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MetadataSchema>> GetMetadataSchema(string entityType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                {
                    return BadRequest(new { message = "Entity type is required" });
                }

                var fields = await _metadataService.GetEntityFieldsAsync(entityType);
                var defaultValues = await _metadataService.GetDefaultEntityMetadataAsync(entityType);

                var schema = new MetadataSchema
                {
                    EntityType = entityType,
                    Fields = fields,
                    DefaultValues = defaultValues,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(schema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata schema for {EntityType}", entityType);
                return StatusCode(500, new { message = "Error retrieving metadata schema" });
            }
        }
    }

    public class MetadataValidationRequest
    {
        public Dictionary<string, object> FieldValues { get; set; } = new();
    }

    public class MetadataTransformRequest
    {
        public Dictionary<string, object> FieldValues { get; set; } = new();
    }

    public class MetadataSchema
    {
        public string EntityType { get; set; } = string.Empty;
        public List<MetadataField> Fields { get; set; } = new();
        public Dictionary<string, object> DefaultValues { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}
