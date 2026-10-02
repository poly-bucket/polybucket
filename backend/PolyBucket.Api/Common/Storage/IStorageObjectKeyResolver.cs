namespace PolyBucket.Api.Common.Storage;

public interface IStorageObjectKeyResolver
{
    string? Resolve(string? storedPathOrUrl);
}
