using PolyBucket.Api.Features.Models.Shared.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.GetModels.Domain
{
    public class GetModelsResponse
    {
        public required IEnumerable<Model> Models { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
} 