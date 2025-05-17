using Core.Entities;
using Core.Models.Models;

namespace Core.Models.Tags
{
    public class Tag : Auditable
    {
        public string Name { get; set; }
        public string Slug { get; set; }

        // Navigation properties
        public List<Model> Models { get; set; } = new List<Model>();

        public List<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
    }
}