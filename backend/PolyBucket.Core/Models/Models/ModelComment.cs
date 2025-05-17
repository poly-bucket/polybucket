using Core.Models.Users;
using System;
using System.Collections.Generic;

namespace Core.Models.Models
{
    public class ModelComment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ModelId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Model Model { get; set; }

        public User User { get; set; }
        public ModelComment ParentComment { get; set; }
        public List<ModelComment> Replies { get; set; } = new List<ModelComment>();
    }
}