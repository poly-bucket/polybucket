using Microsoft.AspNetCore.Http;

namespace PolyBucket.Api.Features.Models.UploadValidation.Domain;

public interface IModelUploadValidator
{
  Task<string?> ValidateAsync(IFormFile[] files, CancellationToken cancellationToken = default);
}