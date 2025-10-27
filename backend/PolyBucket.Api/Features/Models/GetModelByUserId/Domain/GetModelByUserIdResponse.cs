using PolyBucket.Api.Features.Models.Shared.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Domain
{
    public class GetModelByUserIdResponse
    {
        public IEnumerable<Model> Models { get; set; } = new List<Model>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Take { get; set; }
        public int TotalPages { get; set; }
    }
} 