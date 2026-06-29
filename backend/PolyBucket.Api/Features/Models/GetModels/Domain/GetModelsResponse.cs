using PolyBucket.Api.Features.Models.Common;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.GetModels.Domain
{
    public class GetModelsResponse
    {
        public required IEnumerable<ModelDto> Models { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}
