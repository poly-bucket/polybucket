using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataPlugins.Repository;

public interface IGetMetadataPluginsRepository { List<MetadataPluginInfo> GetRegisteredMetadataPlugins(); }
