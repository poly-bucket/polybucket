using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataPlugins.Domain;

public interface IGetMetadataPluginsService { List<MetadataPluginInfo> GetPlugins(); }
