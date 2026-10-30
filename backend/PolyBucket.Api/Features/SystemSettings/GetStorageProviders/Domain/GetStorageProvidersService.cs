using PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Repository;
using PolyBucket.Storage;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Domain;

public class GetStorageProvidersService(IGetStorageProvidersRepository repository) : IGetStorageProvidersService
{
    public StorageProvidersResponseDto GetStorageProviders() => repository.GetProviders();
}
