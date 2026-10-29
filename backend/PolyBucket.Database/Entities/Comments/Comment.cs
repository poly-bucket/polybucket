using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Database.Entities.Comments
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