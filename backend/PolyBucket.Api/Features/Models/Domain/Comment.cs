using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class Comment : Auditable
    {
        public Guid TargetId { get; set; }
        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; }
        public string Content { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
    }
} 