using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class UploadModelController : ControllerBase
    {
        private readonly IStorageService _storage;
        private readonly PolyBucketDbContext _db;
        private readonly ILogger<UploadModelController> _logger;

        public UploadModelController(IStorageService storage, PolyBucketDbContext db, ILogger<UploadModelController> logger)
        {
            _storage = storage;
            _db = db;
            _logger = logger;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Model), StatusCodes.Status201Created)]
        public async Task<ActionResult<Model>> UploadModel([FromForm] ModelUploadRequest request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("File is required");
            }

            // TODO: validate extension & size via settings

            var objectKey = $"models/{Guid.NewGuid()}/{request.File.FileName}";
            await using var stream = request.File.OpenReadStream();
            var url = await _storage.UploadAsync(objectKey, stream, request.File.ContentType, cancellationToken);

            var model = new Model
            {
                Id = Guid.NewGuid(),
                Name = request.Name ?? request.File.FileName,
                Description = request.Description ?? string.Empty,
                FileUrl = url,
                ThumbnailUrl = null,
                AuthorId = Guid.Empty // TODO: derive from JWT claims
            };

            _db.Models.Add(model);
            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(UploadModel), new { id = model.Id }, model);
        }
    }
} 