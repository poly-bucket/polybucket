using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Printers.Domain;
using PolyBucket.Api.Features.Printers.Repository;
using System;

namespace PolyBucket.Api.Features.Printers.Queries
{
    public class PrinterDto
    {
        public Guid Id { get; set; }
        public required string Manufacturer { get; set; }
        public required string Model { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }
        public decimal? PriceUSD { get; set; }
    }

    public class GetPrintersResponse
    {
        public required List<PrinterDto> Printers { get; set; }
    }
    
    public class GetPrintersQueryHandler(IPrintersRepository repository)
    {
        private readonly IPrintersRepository _repository = repository;

        public async Task<GetPrintersResponse> Handle(GetPrintersQuery query, CancellationToken cancellationToken)
        {
            var printers = await _repository.GetPrintersAsync();
            
            var printerDtos = printers.Select(p => new PrinterDto
            {
                Id = p.Id,
                Manufacturer = p.Manufacturer ?? string.Empty,
                Model = p.Model ?? string.Empty,
                Type = p.Type.ToString(),
                Description = p.Description ?? string.Empty,
                PriceUSD = p.PriceUSD
            }).ToList();

            return new GetPrintersResponse { Printers = printerDtos };
        }
    }
} 