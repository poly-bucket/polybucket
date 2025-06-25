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
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal? PriceUSD { get; set; }
    }

    public class GetPrintersResponse
    {
        public List<PrinterDto> Printers { get; set; }
    }
    
    public class GetPrintersQueryHandler
    {
        private readonly IPrintersRepository _repository;

        public GetPrintersQueryHandler(IPrintersRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetPrintersResponse> Handle(GetPrintersQuery query, CancellationToken cancellationToken)
        {
            var printers = await _repository.GetPrintersAsync();
            
            var printerDtos = printers.Select(p => new PrinterDto
            {
                Id = p.Id,
                Manufacturer = p.Manufacturer,
                Model = p.Model,
                Type = p.Type.ToString(),
                Description = p.Description,
                PriceUSD = p.PriceUSD
            }).ToList();

            return new GetPrintersResponse { Printers = printerDtos };
        }
    }
} 