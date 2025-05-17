using Core.Entities;
using Core.Enumerations;
using Core.Models.Users;
using Core.Enums;
using File = Core.Models.Files.File;
using User = Core.Models.Users.User;
using Core.Models.Collections;
using Core.Models.Tags;

namespace Core.Models.Models
{
    public class Model : Auditable
    {
        /// <summary>
        /// The name of the 3D model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the 3D model.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The license type of the 3D model.
        /// </summary>
        public LicenseTypes? License { get; set; }

        /// <summary>
        /// The privacy setting of the 3D model.
        /// </summary>
        public PrivacySettings Privacy { get; set; }

        /// <summary>
        /// List of categories the 3D model belongs under.
        /// </summary>
        public List<ModelCategories> Categories { get; set; }

        /// <summary>
        /// Was this model AI generated?
        /// </summary>
        public bool AIGenerated { get; set; }

        /// <summary>
        /// Is this model a work in progress?
        /// </summary>
        public bool WIP { get; set; }

        /// <summary>
        /// Is this model not safe for work?
        /// </summary>
        public bool NSFW { get; set; }

        /// <summary>
        /// Is this model a remix of another model?
        /// </summary>
        public string IsRemix { get; set; }

        /// <summary>
        /// Whether the model is featured
        /// </summary>
        public bool IsFeatured { get; set; }

        public string VersionLabel { get; set; }

        public Guid? ParentVersionId { get; set; }

        /// <summary>
        /// If rejected, the reason for rejection
        /// </summary>
        public string ModerationReason { get; set; }

        /// <summary>
        /// Date when this model was published (made public)
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Number of downloads
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        /// Number of views
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// Thumbnail URL for quick displays
        /// </summary>
        public string ThumbnailUrl { get; set; }

        #region Navigation Properties

        public virtual User Author { get; set; }

        public virtual ICollection<File> Files { get; set; } = new List<File>();

        public virtual Collection Collection { get; set; }

        public virtual Model ParentVersion { get; set; }

        public virtual ICollection<Model> ChildVersions { get; set; } = new List<Model>();

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

        /// <summary>
        /// Model-Tag relationships
        /// </summary>
        public virtual ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();

        /// <summary>
        /// Current moderation status
        /// </summary>
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;

        #endregion Navigation Properties
    }
}