namespace Core.Models.Filaments
{
    public class Filament
    {
        public string Manufacturer { get; set; }

        public FilamentTypes Type { get; set; }

        public string Color { get; set; }

        public string Diameter { get; set; }
    }
}