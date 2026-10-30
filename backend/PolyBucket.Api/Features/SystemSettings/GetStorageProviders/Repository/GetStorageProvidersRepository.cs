using PolyBucket.Storage;
using StorageGetStorageProviders = PolyBucket.Storage.GetStorageProviders.IGetStorageProvidersService;

namespace PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Repository;

public class GetStorageProvidersRepository(StorageGetStorageProviders storageService) : IGetStorageProvidersRepository
{
    private readonly StorageGetStorageProviders _storageService = storageService;

    public StorageProvidersResponseDto GetProviders() => _storageService.Get();
}
