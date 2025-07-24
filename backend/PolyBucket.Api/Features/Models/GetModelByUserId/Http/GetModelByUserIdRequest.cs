using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Http
{
    public class GetModelByUserIdRequest
    {
        [Range(1, 100)]
        public int Page { get; set; } = 1;
        
        [Range(1, 50)]
        public int Take { get; set; } = 20;
        
        public bool IncludeDeleted { get; set; } = false;
        
        public bool IncludePrivate { get; set; } = false;
    }
} 