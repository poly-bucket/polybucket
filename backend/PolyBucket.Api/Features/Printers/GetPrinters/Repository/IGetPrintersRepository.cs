using PolyBucket.Database.Entities.Printers;

        namespace PolyBucket.Api.Features.Printers.GetPrinters.Repository;

        public interface IGetPrintersRepository
        {
            Task<List<Printer>> GetPrintersAsync(CancellationToken cancellationToken = default);
        }
