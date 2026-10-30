namespace PolyBucket.Api.Features.Printers.GetPrinters.Http;

        public class GetPrintersResponseDto
        {
            public List<PrinterListItemDto> Printers { get; set; } = new();
        }

        public class PrinterListItemDto
        {
            public Guid Id { get; set; }
            public required string Manufacturer { get; set; }
            public required string Model { get; set; }
            public required string Type { get; set; }
            public required string Description { get; set; }
            public decimal? PriceUSD { get; set; }
        }
