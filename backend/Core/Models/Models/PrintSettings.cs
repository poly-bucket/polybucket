namespace Core.Models.Models
{
    public class PrintSettings
    {
        public bool Supports { get; set; }

        public double LayerHeight { get; set; }

        public int WallLoops { get; set; }

        public int InfillPercentage { get; set; }
    }
}