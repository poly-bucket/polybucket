using System;
using System.Collections.Generic;

namespace PolyBucket.Core.Entities
{
    public class Tag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public List<Model> Models { get; set; } = new List<Model>();
        public List<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
    }
} 