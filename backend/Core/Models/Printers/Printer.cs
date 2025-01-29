namespace Core.Models.Printers
{
    public class Printer
    {
        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public int BuildVolumeX { get; set; }

        public int BuildVolumeY { get; set; }

        public int BuildVolumeZ { get; set; }
    }
}