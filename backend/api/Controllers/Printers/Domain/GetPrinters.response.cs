namespace api.Controllers.Printers.Domain;

public class GetPrintersResponse
{
    public List<PrinterDto> Printers { get; set; } = new();
}

public class PrinterDto
{
    public string Id { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PriceUSD { get; set; }
} 