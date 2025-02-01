import { useState, useEffect } from 'react';
import { pluginsService, Plugin } from '../../services/plugins.service';

export const PluginsPage: React.FC = () => {
  const [plugins, setPlugins] = useState<Plugin[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isReloading, setIsReloading] = useState(false);

  useEffect(() => {
    loadPlugins();
  }, []);

  const loadPlugins = async () => {
    try {
      setIsLoading(true);
      const fetchedPlugins = await pluginsService.getPlugins();
      setPlugins(fetchedPlugins);
      setError(null);
    } catch (err) {
      setError('Failed to load plugins');
      console.error('Error loading plugins:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleReloadPlugins = async () => {
    try {
      setIsReloading(true);
      await pluginsService.reloadPlugins();
      await loadPlugins();
    } catch (err) {
      setError('Failed to reload plugins');
      console.error('Error reloading plugins:', err);
    } finally {
      setIsReloading(false);
    }
  };

  if (isLoading) {
    return <div className="text-green-400">Loading plugins...</div>;
  }

  if (error) {
    return <div className="text-red-500">{error}</div>;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-green-400">Plugins</h1>
        <button
          onClick={handleReloadPlugins}
          disabled={isReloading}
          className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50"
        >
          {isReloading ? 'Reloading...' : 'Reload Plugins'}
        </button>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {plugins.map((plugin) => (
          <div
            key={plugin.id}
            className="p-6 bg-gray-900 border rounded-lg border-green-500/30"
          >
            <h2 className="text-xl font-semibold text-green-400 mb-2">
              {plugin.name}
            </h2>
            <p className="text-green-300/80 mb-4">{plugin.description}</p>
            <div className="space-y-1 text-sm text-green-300/60">
              <p>Version: {plugin.version}</p>
              <p>Author: {plugin.author}</p>
              <p>ID: {plugin.id}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}; 