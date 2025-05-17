using Core.Models.Tags;
using System;

namespace Core.Models.Models
{
    public class ModelTag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ModelId { get; set; }
        public Guid TagId { get; set; }

        // Navigation properties
        public Model Model { get; set; }

        public Tag Tag { get; set; }
    }
}