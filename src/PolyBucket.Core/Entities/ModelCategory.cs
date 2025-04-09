using System;

namespace PolyBucket.Core.Entities
{
    public class ModelCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ModelId { get; set; }
        public Guid CategoryId { get; set; }
        
        // Navigation properties
        public Model Model { get; set; }
        public Category Category { get; set; }
    }
} 