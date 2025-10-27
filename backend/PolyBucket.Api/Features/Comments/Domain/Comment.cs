using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Shared.Domain;

namespace PolyBucket.Api.Features.Comments.Domain
{
    public class Comment : Auditable
    {
        public required string Content { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public virtual required Model Model { get; set; }
        public virtual required User Author { get; set; }
    }
} 