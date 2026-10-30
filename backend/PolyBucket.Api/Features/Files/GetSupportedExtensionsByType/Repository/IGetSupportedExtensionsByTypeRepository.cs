namespace PolyBucket.Api.Features.Files.GetSupportedExtensionsByType.Repository;

        public interface IGetSupportedExtensionsByTypeRepository
        {
            Task<List<string>> GetExtensionsForTypeAsync(string fileType, CancellationToken cancellationToken = default);
        }
