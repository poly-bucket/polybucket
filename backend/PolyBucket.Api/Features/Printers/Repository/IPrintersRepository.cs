using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Printers;

namespace PolyBucket.Api.Features.Printers.Repository
{
    public interface IPrintersRepository
    {
        Task<List<Printer>> GetPrintersAsync();
    }
} 