using Core.Entities;
using System;
using System.Collections.Generic;

namespace Core.Models.Catagories
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string IconUrl { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Category ParentCategory { get; set; }

        public List<Category> ChildCategories { get; set; } = new List<Category>();
        public List<ModelCategory> ModelCategories { get; set; } = new List<ModelCategory>();
        public List<Model> Models { get; set; } = new List<Model>();
    }
}