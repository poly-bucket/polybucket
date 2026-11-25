using PolyBucket.Database.Entities.Printers;

namespace PolyBucket.Api.Features.Printers;

public static class PrinterCatalog
{
    public static readonly Guid PrusaMk3sPlusId = Guid.Parse("a8f3c201-4b2e-4d91-9f16-8c4e5a6b7d01");
    public static readonly Guid BambuX1CarbonId = Guid.Parse("b9e4d302-5c3f-4e02-a027-9d5f6b7c8e12");

    public static IReadOnlyList<Printer> GetPrinters()
    {
        return new List<Printer>
        {
            new()
            {
                Id = PrusaMk3sPlusId,
                Manufacturer = "Prusa Research",
                Model = "i3 MK3S+",
                Type = PrinterType.FDM,
                BuildVolumeX = 250,
                BuildVolumeY = 210,
                BuildVolumeZ = 210,
                Description = "The Original Prusa i3 MK3S+ is the latest version of their award-winning 3D printer.",
                PriceUSD = 749m
            },
            new()
            {
                Id = BambuX1CarbonId,
                Manufacturer = "Bambu Lab",
                Model = "X1 Carbon",
                Type = PrinterType.FDM,
                BuildVolumeX = 256,
                BuildVolumeY = 256,
                BuildVolumeZ = 256,
                Description = "High-speed CoreXY printer with advanced features.",
                PriceUSD = 1199m
            }
        };
    }

    public static bool Contains(Guid printerId)
    {
        return printerId == PrusaMk3sPlusId || printerId == BambuX1CarbonId;
    }

    public static Printer? FindById(Guid printerId)
    {
        return GetPrinters().FirstOrDefault(p => p.Id == printerId);
    }
}