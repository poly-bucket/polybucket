using PolyBucket.Api.Features.Printers.GetPrinters.Http;
        using PolyBucket.Api.Features.Printers.GetPrinters.Repository;

        namespace PolyBucket.Api.Features.Printers.GetPrinters.Domain;

        public interface IGetPrintersService
        {
            Task<GetPrintersResponseDto> GetPrintersAsync(CancellationToken cancellationToken = default);
        }

        public class GetPrintersService(IGetPrintersRepository repository) : IGetPrintersService
        {
            public async Task<GetPrintersResponseDto> GetPrintersAsync(CancellationToken cancellationToken = default)
            {
                var printers = await repository.GetPrintersAsync(cancellationToken);
                return new GetPrintersResponseDto
                {
                    Printers = printers.Select(p => new PrinterListItemDto
                    {
                        Id = p.Id,
                        Manufacturer = p.Manufacturer ?? string.Empty,
                        Model = p.Model ?? string.Empty,
                        Type = p.Type.ToString(),
                        Description = p.Description ?? string.Empty,
                        PriceUSD = p.PriceUSD
                    }).ToList()
                };
            }
        }
