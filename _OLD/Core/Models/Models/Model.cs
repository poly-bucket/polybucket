using Core.Enumerations;
using Core.Extensions.Models;
using Core.Models.Users;
using File = Core.Models.Files.File;

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

        #region Navigation Properties

        public virtual User Author { get; set; }

        public IEnumerable<File> Files { get; set; }

        #endregion Navigation Properties
    }
}