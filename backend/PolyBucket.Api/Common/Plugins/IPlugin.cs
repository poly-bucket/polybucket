using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace PolyBucket.Api.Common.Plugins
{
    [InheritedExport]
    public interface IPlugin
    {
        string Id { get; }
        string Name { get; }
        string Version { get; }
        string Author { get; }
        string Description { get; }
        Task InitializeAsync();
        Task UnloadAsync();
    }
} 