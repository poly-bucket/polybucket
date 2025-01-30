using Core.Models.Enumerations.Printers;
using Core.Models.Printers;
using Microsoft.EntityFrameworkCore;

namespace Database.Seeders;

public class PrinterSeeder
{
    private readonly Context _context;

    public PrinterSeeder(Context context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Printers.AnyAsync())
            return;

        var printers = GetPrinters();
        await _context.Printers.AddRangeAsync(printers);
        await _context.SaveChangesAsync();
    }

    private static List<Printer> GetPrinters()
    {
        var now = DateTime.UtcNow;
        return new List<Printer>
        {
            // Prusa Research Printers
            new Printer
            {
                Manufacturer = "Prusa Research",
                Model = "i3 MK3S+",
                BuildVolumeX = 250,
                BuildVolumeY = 210,
                BuildVolumeZ = 210,
                MaxBedTemp = 120,
                MaxNozzleTemp = 280,
                HasHeatedBed = true,
                HasHeatedChamber = false,
                HasEnclosure = false,
                DefaultNozzleDiameter = 0.4m,
                ExtruderCount = 1,
                ExtruderType = ExtruderType.DirectDrive,
                Connectivity = new List<ConnectivityType> { ConnectivityType.USB, ConnectivityType.SDCard, ConnectivityType.WiFi },
                SupportedMaterials = new List<MaterialType> 
                { 
                    MaterialType.PLA, 
                    MaterialType.PETG, 
                    MaterialType.ABS, 
                    MaterialType.TPU,
                    MaterialType.ASA,
                    MaterialType.PC
                },
                PriceUSD = 749m,
                ReleaseYear = 2020,
                Type = PrinterType.FDM,
                HasAutoLeveling = true,
                HasFilamentSensor = true,
                HasPowerLossContinue = true,
                MaxPrintSpeed = 200,
                WebsiteUrl = "https://www.prusa3d.com/product/original-prusa-i3-mk3s-kit-3/",
                Description = "The Original Prusa i3 MK3S+ is the latest version of their award-winning 3D printer. Known for reliability and print quality.",
                CreatedAt = now,
                UpdatedAt = now
            },

            new Printer
            {
                Manufacturer = "Prusa Research",
                Model = "MINI+",
                BuildVolumeX = 180,
                BuildVolumeY = 180,
                BuildVolumeZ = 180,
                MaxBedTemp = 100,
                MaxNozzleTemp = 280,
                HasHeatedBed = true,
                HasHeatedChamber = false,
                HasEnclosure = false,
                DefaultNozzleDiameter = 0.4m,
                ExtruderCount = 1,
                ExtruderType = ExtruderType.Bowden,
                Connectivity = new List<ConnectivityType> { ConnectivityType.USB, ConnectivityType.WiFi, ConnectivityType.LCD },
                SupportedMaterials = new List<MaterialType> 
                { 
                    MaterialType.PLA, 
                    MaterialType.PETG, 
                    MaterialType.ASA
                },
                PriceUSD = 399m,
                ReleaseYear = 2021,
                Type = PrinterType.FDM,
                HasAutoLeveling = true,
                HasFilamentSensor = true,
                HasPowerLossContinue = true,
                MaxPrintSpeed = 200,
                WebsiteUrl = "https://www.prusa3d.com/product/original-prusa-mini-kit-3/",
                Description = "Compact and affordable printer with many features from its bigger brother, the MK3S+.",
                CreatedAt = now,
                UpdatedAt = now
            },

            new Printer
            {
                Manufacturer = "Prusa Research",
                Model = "XL",
                BuildVolumeX = 360,
                BuildVolumeY = 360,
                BuildVolumeZ = 360,
                MaxBedTemp = 120,
                MaxNozzleTemp = 280,
                HasHeatedBed = true,
                HasHeatedChamber = false,
                HasEnclosure = true,
                DefaultNozzleDiameter = 0.4m,
                ExtruderCount = 5,
                ExtruderType = ExtruderType.DirectDrive,
                Connectivity = new List<ConnectivityType> { ConnectivityType.USB, ConnectivityType.WiFi, ConnectivityType.Ethernet },
                SupportedMaterials = new List<MaterialType> 
                { 
                    MaterialType.PLA, 
                    MaterialType.PETG, 
                    MaterialType.ABS,
                    MaterialType.TPU,
                    MaterialType.PC,
                    MaterialType.ASA,
                    MaterialType.Carbon
                },
                PriceUSD = 1999m,
                ReleaseYear = 2022,
                Type = PrinterType.FDM,
                HasAutoLeveling = true,
                HasFilamentSensor = true,
                HasPowerLossContinue = true,
                MaxPrintSpeed = 250,
                WebsiteUrl = "https://www.prusa3d.com/product/original-prusa-xl-3/",
                Description = "Large-format printer with tool-changing capability and up to 5 independent extruders.",
                CreatedAt = now,
                UpdatedAt = now
            },

            // Creality Printers
            new Printer
            {
                Manufacturer = "Creality",
                Model = "Ender 3 V2",
                BuildVolumeX = 220,
                BuildVolumeY = 220,
                BuildVolumeZ = 250,
                MaxBedTemp = 100,
                MaxNozzleTemp = 260,
                HasHeatedBed = true,
                HasHeatedChamber = false,
                HasEnclosure = false,
                DefaultNozzleDiameter = 0.4m,
                ExtruderCount = 1,
                ExtruderType = ExtruderType.Bowden,
                Connectivity = new List<ConnectivityType> { ConnectivityType.USB, ConnectivityType.SDCard },
                SupportedMaterials = new List<MaterialType> 
                { 
                    MaterialType.PLA, 
                    MaterialType.PETG, 
                    MaterialType.ABS,
                    MaterialType.TPU
                },
                PriceUSD = 279m,
                ReleaseYear = 2020,
                Type = PrinterType.FDM,
                HasAutoLeveling = false,
                HasFilamentSensor = false,
                HasPowerLossContinue = true,
                MaxPrintSpeed = 180,
                WebsiteUrl = "https://www.creality.com/products/ender-3-v2-3d-printer",
                Description = "Popular entry-level 3D printer with good print quality and a strong community.",
                CreatedAt = now,
                UpdatedAt = now
            },

            // Bambu Lab Printers
            new Printer
            {
                Manufacturer = "Bambu Lab",
                Model = "X1 Carbon",
                BuildVolumeX = 256,
                BuildVolumeY = 256,
                BuildVolumeZ = 256,
                MaxBedTemp = 120,
                MaxNozzleTemp = 300,
                HasHeatedBed = true,
                HasHeatedChamber = true,
                HasEnclosure = true,
                DefaultNozzleDiameter = 0.4m,
                ExtruderCount = 1,
                ExtruderType = ExtruderType.DirectDrive,
                Connectivity = new List<ConnectivityType> { ConnectivityType.USB, ConnectivityType.WiFi, ConnectivityType.Ethernet },
                SupportedMaterials = new List<MaterialType> 
                { 
                    MaterialType.PLA, 
                    MaterialType.PETG, 
                    MaterialType.ABS,
                    MaterialType.TPU,
                    MaterialType.PC,
                    MaterialType.ASA,
                    MaterialType.PA,
                    MaterialType.Carbon
                },
                PriceUSD = 1199m,
                ReleaseYear = 2022,
                Type = PrinterType.FDM,
                HasAutoLeveling = true,
                HasFilamentSensor = true,
                HasPowerLossContinue = true,
                MaxPrintSpeed = 500,
                WebsiteUrl = "https://bambulab.com/en/x1-carbon",
                Description = "High-speed CoreXY printer with advanced features like AI monitoring and automated calibration.",
                CreatedAt = now,
                UpdatedAt = now
            },

            // Voron Design
            new Printer
            {
                Manufacturer = "Voron Design",
                Model = "2.4",
                BuildVolumeX = 350,
                BuildVolumeY = 350,
                BuildVolumeZ = 350,
                MaxBedTemp = 130,
                MaxNozzleTemp = 300,
                HasHeatedBed = true,
                HasHeatedChamber = true,
                HasEnclosure = true,
                DefaultNozzleDiameter = 0.4m,
                ExtruderCount = 1,
                ExtruderType = ExtruderType.DirectDrive,
                Connectivity = new List<ConnectivityType> { ConnectivityType.USB, ConnectivityType.WiFi },
                SupportedMaterials = new List<MaterialType> 
                { 
                    MaterialType.PLA, 
                    MaterialType.PETG, 
                    MaterialType.ABS,
                    MaterialType.TPU,
                    MaterialType.PC,
                    MaterialType.ASA,
                    MaterialType.PA
                },
                PriceUSD = 1500m,
                ReleaseYear = 2020,
                Type = PrinterType.FDM,
                HasAutoLeveling = true,
                HasFilamentSensor = true,
                HasPowerLossContinue = true,
                MaxPrintSpeed = 300,
                WebsiteUrl = "https://vorondesign.com/voron2.4",
                Description = "Open-source CoreXY printer known for high-speed, high-temperature printing capabilities.",
                CreatedAt = now,
                UpdatedAt = now
            },

            // Formlabs
            new Printer
            {
                Manufacturer = "Formlabs",
                Model = "Form 3+",
                BuildVolumeX = 145,
                BuildVolumeY = 145,
                BuildVolumeZ = 185,
                MaxBedTemp = 0,
                MaxNozzleTemp = 0,
                HasHeatedBed = false,
                HasHeatedChamber = false,
                HasEnclosure = true,
                DefaultNozzleDiameter = 0,
                ExtruderCount = 0,
                ExtruderType = ExtruderType.SLA,
                Connectivity = new List<ConnectivityType> { ConnectivityType.USB, ConnectivityType.WiFi, ConnectivityType.Ethernet },
                SupportedMaterials = new List<MaterialType> 
                { 
                    MaterialType.Resin
                },
                PriceUSD = 3499m,
                ReleaseYear = 2021,
                Type = PrinterType.SLA,
                HasAutoLeveling = true,
                HasFilamentSensor = false,
                HasPowerLossContinue = true,
                MaxPrintSpeed = 0,
                WebsiteUrl = "https://formlabs.com/3d-printers/form-3/",
                Description = "Professional SLA printer known for high-detail prints and extensive material options.",
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }
} 