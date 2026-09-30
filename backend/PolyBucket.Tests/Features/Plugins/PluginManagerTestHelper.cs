using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Tests.Features.Plugins;

internal static class PluginManagerTestHelper
{
    public static PluginManager WithPlugins(params IPlugin[] plugins)
    {
        var manager = new PluginManager(Path.Combine(Path.GetTempPath(), "polybucket-plugin-tests", Guid.NewGuid().ToString("N")));
        var field = typeof(PluginManager).GetField("_loadedPlugins", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<IPlugin>)field!.GetValue(manager)!;
        list.AddRange(plugins);
        return manager;
    }
}
