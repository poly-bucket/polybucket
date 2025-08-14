import React, { useState, useEffect } from 'react';
import { Extension as ExtensionIcon } from '@mui/icons-material';

const PluginsTab: React.FC = () => {
  const [plugins, setPlugins] = useState([
    { id: 'comments', name: 'Comments Plugin', status: 'enabled', version: '1.0.0' },
    { id: 'federation', name: 'Federation Plugin', status: 'disabled', version: '1.2.0' },
    { id: 'analytics', name: 'Analytics Plugin', status: 'enabled', version: '2.1.0' },
  ]);

  const [selectedPlugin, setSelectedPlugin] = useState<string | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [hooksOpen, setHooksOpen] = useState(false);

  const loadPluginData = async () => {
    // Mock API call
    console.log('Loading plugin data...');
  };

  useEffect(() => {
    loadPluginData();
  }, []);

  const handlePluginSelect = async (pluginId: string) => {
    setSelectedPlugin(pluginId);
    console.log('Selected plugin:', pluginId);
  };

  const handleTogglePlugin = async (pluginId: string, currentStatus: string) => {
    const newStatus = currentStatus === 'enabled' ? 'disabled' : 'enabled';
    setPlugins(prev => prev.map(plugin => 
      plugin.id === pluginId ? { ...plugin, status: newStatus } : plugin
    ));
    console.log(`Plugin ${pluginId} ${newStatus}`);
  };

  const handleOpenSettings = async (pluginId: string) => {
    setSelectedPlugin(pluginId);
    setSettingsOpen(true);
  };

  const handleOpenHooks = async () => {
    setHooksOpen(true);
  };

  const handleSaveSettings = async (settings: Record<string, any>) => {
    console.log('Saving plugin settings:', settings);
    setSettingsOpen(false);
  };

  const handleReloadPlugins = async () => {
    console.log('Reloading plugins...');
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">Plugin Management</h2>
        <button
          onClick={handleReloadPlugins}
          className="lg-button lg-button-secondary"
        >
          Reload Plugins
        </button>
      </div>

      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Installed Plugins</h3>
        
        <div className="space-y-4">
          {plugins.map((plugin) => (
            <div key={plugin.id} className="flex items-center justify-between p-4 bg-white/5 rounded-lg">
              <div className="flex items-center space-x-3">
                <div className="w-10 h-10 bg-indigo-500 rounded flex items-center justify-center">
                  <ExtensionIcon className="w-6 h-6 text-white" />
                </div>
                <div>
                  <div className="text-white font-medium">{plugin.name}</div>
                  <div className="text-sm text-white/60">Version {plugin.version}</div>
                </div>
              </div>
              
              <div className="flex items-center space-x-2">
                <span className={`lg-badge ${
                  plugin.status === 'enabled' ? 'lg-badge-success' : 'lg-badge-error'
                }`}>
                  {plugin.status}
                </span>
                
                <button
                  onClick={() => handleTogglePlugin(plugin.id, plugin.status)}
                  className={`lg-button text-sm ${
                    plugin.status === 'enabled' ? 'lg-button-secondary' : 'lg-button-primary'
                  }`}
                >
                  {plugin.status === 'enabled' ? 'Disable' : 'Enable'}
                </button>
                
                <button
                  onClick={() => handleOpenSettings(plugin.id)}
                  className="lg-button text-sm"
                >
                  Settings
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Plugin Hooks</h3>
        <p className="text-white/60 mb-4">
          Configure how plugins interact with the system through hooks and events.
        </p>
        <button
          onClick={handleOpenHooks}
          className="lg-button lg-button-primary"
        >
          Configure Hooks
        </button>
      </div>

      {/* Plugin Settings Dialog */}
      {settingsOpen && selectedPlugin && (
        <div className="lg-modal-overlay">
          <div className="lg-modal p-6 max-w-2xl w-full">
            <h3 className="text-xl font-bold text-white mb-4">
              {plugins.find(p => p.id === selectedPlugin)?.name} Settings
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">API Key</label>
                <input
                  type="text"
                  className="lg-input"
                  placeholder="Enter API key..."
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Webhook URL</label>
                <input
                  type="url"
                  className="lg-input"
                  placeholder="https://example.com/webhook"
                />
              </div>
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="enableDebug"
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
                />
                <label htmlFor="enableDebug" className="ml-2 text-sm text-white/80">
                  Enable debug mode
                </label>
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setSettingsOpen(false)}
                className="lg-button"
              >
                Cancel
              </button>
              <button
                onClick={() => handleSaveSettings({})}
                className="lg-button lg-button-primary"
              >
                Save Settings
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Plugin Hooks Dialog */}
      {hooksOpen && (
        <div className="lg-modal-overlay">
          <div className="lg-modal p-6 max-w-4xl w-full">
            <h3 className="text-xl font-bold text-white mb-4">Plugin Hooks Configuration</h3>
            
            <div className="space-y-4">
              <div className="lg-card p-4">
                <h4 className="text-lg font-medium text-white mb-2">Model Events</h4>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onModelUpload</span>
                    <select className="lg-input w-48">
                      <option>Comments Plugin</option>
                      <option>Analytics Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onModelDelete</span>
                    <select className="lg-input w-48">
                      <option>Analytics Plugin</option>
                      <option>Comments Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                </div>
              </div>
              
              <div className="lg-card p-4">
                <h4 className="text-lg font-medium text-white mb-2">User Events</h4>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onUserLogin</span>
                    <select className="lg-input w-48">
                      <option>Analytics Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onUserRegister</span>
                    <select className="lg-input w-48">
                      <option>Analytics Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                </div>
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setHooksOpen(false)}
                className="lg-button"
              >
                Cancel
              </button>
              <button
                onClick={() => setHooksOpen(false)}
                className="lg-button lg-button-primary"
              >
                Save Configuration
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PluginsTab;
