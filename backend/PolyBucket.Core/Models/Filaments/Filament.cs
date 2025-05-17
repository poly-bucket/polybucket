using Core.Entities;
using Core.Models.Enumerations.Printers;

namespace Core.Models.Filaments
{
    public class Filament : BaseEntity
    {
        public string Manufacturer { get; set; }

        public MaterialType Type { get; set; }

        public string Color { get; set; }

        public string Diameter { get; set; }
    }
}