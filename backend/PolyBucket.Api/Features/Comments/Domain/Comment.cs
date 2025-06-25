using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Domain;

namespace PolyBucket.Api.Features.Comments.Domain
{
    public class Comment : Auditable
    {
        public string Content { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public virtual Model Model { get; set; }
        public virtual User Author { get; set; }
    }
} 