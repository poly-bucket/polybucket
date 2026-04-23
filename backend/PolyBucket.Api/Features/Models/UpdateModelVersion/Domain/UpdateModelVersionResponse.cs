using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;

namespace PolyBucket.Api.Features.Models.UpdateModelVersion.Domain
{
    public class UpdateModelVersionResponse
    {
        public ModelVersion ModelVersion { get; set; } = null!;
    }
} 