namespace PolyBucket.Api.Features.Files.GetSupportedExtensions.Repository;

        public interface IGetSupportedExtensionsRepository
        {
            Task<List<string>> GetExtensionsAsync(CancellationToken cancellationToken = default);
        }
