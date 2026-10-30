namespace PolyBucket.Storage.GetStorageProviders;

public interface IGetStorageProvidersService
{
    StorageProvidersResponseDto Get();
}

public sealed class GetStorageProvidersService : IGetStorageProvidersService
{
    public StorageProvidersResponseDto Get()
    {
        var providers = StorageProviderCatalog.GetAll()
            .Select(definition => new StorageProviderCatalogEntryDto
            {
                Id = definition.Id,
                DisplayName = definition.DisplayName,
                Description = definition.Description,
                Defaults = new StorageProviderDefaultsDto
                {
                    Endpoint = definition.Defaults.Endpoint,
                    Port = definition.Defaults.Port,
                    UseSsl = definition.Defaults.UseSsl,
                    BucketName = definition.Defaults.BucketName,
                    ExternalEndpoint = definition.Defaults.ExternalEndpoint,
                    ExternalPort = definition.Defaults.ExternalPort,
                    ExternalUseSsl = definition.Defaults.ExternalUseSsl,
                    Region = definition.Defaults.Region,
                    ForcePathStyle = definition.Defaults.ForcePathStyle
                }
            })
            .ToList();

        return new StorageProvidersResponseDto { Providers = providers };
    }
}
