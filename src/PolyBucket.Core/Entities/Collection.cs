using System;
using System.Collections.Generic;

namespace PolyBucket.Core.Entities
{
    public class Collection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPublic { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public User User { get; set; }
        public List<Model> Models { get; set; } = new List<Model>();
    }
} 