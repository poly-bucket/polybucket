using System.Linq;
using PolyBucket.Api.Features.Printers;
using PolyBucket.Database.Entities.Printers;

namespace PolyBucket.Api.Features.Printers.GetPrinters.Repository;

public class GetPrintersRepository : IGetPrintersRepository
{
    public Task<List<Printer>> GetPrintersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PrinterCatalog.GetPrinters().ToList());
    }
}
