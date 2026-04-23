using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.CommentModel.Domain
{
    public class Comment : Auditable
    {
        public new Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;
        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
