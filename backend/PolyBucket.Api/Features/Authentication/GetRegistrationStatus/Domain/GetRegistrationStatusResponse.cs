namespace PolyBucket.Api.Features.Authentication.GetRegistrationStatus.Domain;

public interface IGetRegistrationStatusService
{
    Task<GetRegistrationStatusResponse> GetRegistrationStatusAsync(CancellationToken cancellationToken);
}

public class GetRegistrationStatusResponse
{
    public bool AllowUserRegistration { get; set; }
}
