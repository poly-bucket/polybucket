namespace api.Controllers.Printers.Domain;

public interface IGetPrintersService
{
    Task<GetPrintersResponse> ExecuteAsync();
} 