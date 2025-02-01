using Microsoft.EntityFrameworkCore;
using Database;
using Core.Models.Printers;

namespace api.Controllers.Printers.Domain;

public class GetPrintersService : IGetPrintersService
{
    private readonly Context _context;

    public GetPrintersService(Context context)
    {
        _context = context;
    }

    public async Task<GetPrintersResponse> ExecuteAsync()
    {
        var printers = await _context.Printers
            .Select(p => new PrinterDto
            {
                Id = p.Id.ToString(),
                Manufacturer = p.Manufacturer,
                Model = p.Model,
                Type = p.Type.ToString(),
                Description = p.Description ?? string.Empty,
                PriceUSD = p.PriceUSD
            })
            .ToListAsync();

        return new GetPrintersResponse
        {
            Printers = printers
        };
    }
} 