namespace Core.Enumerations
{
    /// <summary>
    /// Represents the supported file extensions in the system
    /// </summary>
    public enum FileExtension
    {
        None = 0,

        #region 3D Model File Extensions

        Stl = 1,
        Obj = 2,
        Fbx = 3,
        Gltf = 4,
        Glb = 5,
        ThreeMf = 6, // 3MF format
        Ply = 7,
        Step = 8,
        Stp = 9,
        Iges = 10,
        Igs = 11,
        Brep = 12,
        Gcode = 13,
        Scad = 14,
        Amf = 15,
        Blend = 16,
        Max = 17,
        Dae = 18, // Collada

        #endregion 3D Model File Extensions

        #region Image File Extensions

        Jpg = 100,
        Jpeg = 101,
        Png = 102,
        Gif = 103,
        Bmp = 104,
        Tiff = 105,
        Webp = 106,
        Tga = 107,
        Hdr = 108,
        Exr = 109,

        #endregion Image File Extensions

        #region Vector File Extensions

        Ai = 200,
        Eps = 201,
        Pdf = 202,
        Svg = 203,
        Dxf = 204,

        #endregion Vector File Extensions

        #region Document File Extensions

        Doc = 300,
        Docx = 301,
        Txt = 302,
        Rtf = 303,
        Md = 304,

        #endregion Document File Extensions

        #region Archive File Extensions

        Zip = 400,
        Rar = 401,
        SevenZ = 402,
        Tar = 403,
        TarGz = 404,

        #endregion Archive File Extensions

        #region Other File Extensions

        Json = 500,
        Xml = 501,
        Yaml = 502,
        Csv = 503,

        #endregion Other File Extensions
    }
}