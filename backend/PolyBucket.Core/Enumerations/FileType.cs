namespace Core.Enumerations
{
    /// <summary>
    /// Describes the purpose of a file in relation to a model
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Default file type
        /// </summary>
        Other = 0,

        /// <summary>
        /// The primary 3D model file
        /// </summary>
        MainModel = 1,

        /// <summary>
        /// A thumbnail or preview image
        /// </summary>
        Thumbnail = 2,

        /// <summary>
        /// A high-quality render of the model
        /// </summary>
        Render = 3,

        /// <summary>
        /// Source file for the model (e.g., Blender, Maya, FreeCAD)
        /// </summary>
        SourceFile = 4,

        /// <summary>
        /// Documentation for the model (instructions, guides)
        /// </summary>
        Documentation = 5,

        /// <summary>
        /// Print settings file (slicer settings, profiles)
        /// </summary>
        PrintSettings = 6,

        /// <summary>
        /// Material definitions or textures
        /// </summary>
        Material = 7,

        /// <summary>
        /// A license file
        /// </summary>
        License = 8,

        /// <summary>
        /// G-code or other machine-specific instructions
        /// </summary>
        MachineCode = 9,

        /// <summary>
        /// Alternative model format
        /// </summary>
        AlternativeFormat = 10
    }
}