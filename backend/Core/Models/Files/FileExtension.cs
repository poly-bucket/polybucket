using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Files
{
    public enum FileExtension
    {
        None,

        #region 3D Model File Extensions

        obj,
        stl,
        threemf,
        gcode,

        #endregion 3D Model File Extensions

        #region Image File Extensions

        jpg,
        jpeg,
        png,
        gif,
        bmp,
        tiff,

        #endregion Image File Extensions

        #region Vector File Extensions

        ai,
        eps,
        pdf,
        svg,

        #endregion Vector File Extensions
    }
}