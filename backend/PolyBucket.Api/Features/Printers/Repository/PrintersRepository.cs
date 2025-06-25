using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Printers.Domain;

namespace PolyBucket.Api.Features.Printers.Repository
{
    public class PrintersRepository : IPrintersRepository
    {
        public Task<List<Printer>> GetPrintersAsync()
        {
            var printers = new List<Printer>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Manufacturer = "Prusa Research",
                    Model = "i3 MK3S+",
                    Type = PrinterType.FDM,
                    Description = "The Original Prusa i3 MK3S+ is the latest version of their award-winning 3D printer.",
                    PriceUSD = 749m
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Manufacturer = "Bambu Lab",
                    Model = "X1 Carbon",
                    Type = PrinterType.FDM,
                    Description = "High-speed CoreXY printer with advanced features.",
                    PriceUSD = 1199m
                }
            };

            return Task.FromResult(printers);
        }
    }
} 