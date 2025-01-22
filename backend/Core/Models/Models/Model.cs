namespace Core.Models.Models
{
    public class Model : Auditable.Auditable
    {
        /// <summary>
        /// The name of the 3D model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the 3D model.
        /// </summary>
        public string Description { get; set; }
    }
}