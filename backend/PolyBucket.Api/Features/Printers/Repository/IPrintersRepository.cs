using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Printers.Domain;

namespace PolyBucket.Api.Features.Printers.Repository
{
    public interface IPrintersRepository
    {
        Task<List<Printer>> GetPrintersAsync();
    }
} 