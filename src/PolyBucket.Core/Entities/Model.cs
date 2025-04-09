using System;
using System.Collections.Generic;
using PolyBucket.Core.Enums;

namespace PolyBucket.Core.Entities
{
    /// <summary>
    /// Represents a 3D model in the system
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Unique identifier for the model
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Name of the model
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the model
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// User who uploaded the model
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Navigation property to the user
        /// </summary>
        public virtual User User { get; set; }
        
        /// <summary>
        /// License type for the model
        /// </summary>
        public string License { get; set; }
        
        /// <summary>
        /// Thumbnail URL for quick displays
        /// </summary>
        public string ThumbnailUrl { get; set; }
        
        /// <summary>
        /// Collection ID if this model belongs to a collection
        /// </summary>
        public Guid? CollectionId { get; set; }
        
        /// <summary>
        /// Navigation property to the collection
        /// </summary>
        public virtual Collection Collection { get; set; }
        
        /// <summary>
        /// Current version label
        /// </summary>
        public string VersionLabel { get; set; }
        
        /// <summary>
        /// Parent version ID if this is a version of another model
        /// </summary>
        public Guid? ParentVersionId { get; set; }
        
        /// <summary>
        /// Navigation property to the parent version
        /// </summary>
        public virtual Model ParentVersion { get; set; }
        
        /// <summary>
        /// Child versions of this model
        /// </summary>
        public virtual ICollection<Model> ChildVersions { get; set; } = new List<Model>();
        
        /// <summary>
        /// All files associated with this model
        /// </summary>
        public virtual ICollection<File> Files { get; set; } = new List<File>();
        
        /// <summary>
        /// Tags associated with this model
        /// </summary>
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

        /// <summary>
        /// Model-Tag relationships
        /// </summary>
        public virtual ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
        
        /// <summary>
        /// Categories this model belongs to
        /// </summary>
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        /// <summary>
        /// Model-Category relationships
        /// </summary>
        public virtual ICollection<ModelCategory> ModelCategories { get; set; } = new List<ModelCategory>();
        
        /// <summary>
        /// Current moderation status
        /// </summary>
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;
        
        /// <summary>
        /// If rejected, the reason for rejection
        /// </summary>
        public string ModerationReason { get; set; }
        
        /// <summary>
        /// Date and time when the model was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Date and time when the model was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Date when this model was published (made public)
        /// </summary>
        public DateTime? PublishedAt { get; set; }
        
        /// <summary>
        /// Format of the main file (e.g., STL, OBJ)
        /// </summary>
        public FileExtension MainFileFormat { get; set; }
        
        /// <summary>
        /// Whether the model is featured
        /// </summary>
        public bool IsFeatured { get; set; }
        
        /// <summary>
        /// Whether the model is public
        /// </summary>
        public bool IsPublic { get; set; }
        
        /// <summary>
        /// Number of downloads
        /// </summary>
        public int DownloadCount { get; set; }
        
        /// <summary>
        /// Number of views
        /// </summary>
        public int ViewCount { get; set; }
        
        /// <summary>
        /// Number of likes
        /// </summary>
        public int LikeCount { get; set; }
    }
} 