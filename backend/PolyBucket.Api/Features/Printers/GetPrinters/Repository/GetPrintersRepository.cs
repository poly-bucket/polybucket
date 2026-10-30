using PolyBucket.Database.Entities.Printers;

        namespace PolyBucket.Api.Features.Printers.GetPrinters.Repository;

        public class GetPrintersRepository : IGetPrintersRepository
        {
            public Task<List<Printer>> GetPrintersAsync(CancellationToken cancellationToken = default)
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
