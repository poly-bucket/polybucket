import React, { useState, useEffect } from 'react';
import { usePluginRegistry } from '../../plugins/PluginRegistry';

interface PluginInfo {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  metadata: {
    minimumAppVersion: string;
    maximumAppVersion?: string;
    requiredPermissions: string[];
    optionalPermissions: string[];
    lifecycle: {
      autoStart: boolean;
      canDisable: boolean;
      canUninstall: boolean;
    };
  };
  frontendComponents: Array<{
    id: string;
    name: string;
    type: string;
    hooks: Array<{
      hookName: string;
      priority: number;
    }>;
  }>;
  isActive: boolean;
  lastUpdated?: string;
}

const PluginManagement: React.FC = () => {
  const pluginRegistry = usePluginRegistry();
  const [plugins, setPlugins] = useState<PluginInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [selectedPlugin, setSelectedPlugin] = useState<PluginInfo | null>(null);
  const [showDetails, setShowDetails] = useState(false);

  useEffect(() => {
    loadPlugins();
  }, []);

  const loadPlugins = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/plugins');
      
      if (response.ok) {
        const pluginData = await response.json();
        const registryPlugins = pluginRegistry.getPlugins();
        
        // Combine backend plugin info with frontend registry status
        const enhancedPlugins = pluginData.map((plugin: any) => ({
          ...plugin,
          isActive: registryPlugins.some(p => p.id === plugin.id),
          frontendComponents: registryPlugins.find(p => p.id === plugin.id)?.frontendComponents || []
        }));
        
        setPlugins(enhancedPlugins);
      } else {
        setError('Failed to load plugins');
      }
    } catch (err) {
      setError('Failed to load plugins');
      console.error('Error loading plugins:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleReloadPlugins = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/plugins/reload', { method: 'POST' });
      
      if (response.ok) {
        // Reinitialize frontend plugins
        await pluginRegistry.initializeFromBackend();
        await loadPlugins();
      } else {
        setError('Failed to reload plugins');
      }
    } catch (err) {
      setError('Failed to reload plugins');
      console.error('Error reloading plugins:', err);
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (plugin: PluginInfo) => {
    if (!plugin.isActive) return 'text-red-600 bg-red-100';
    return 'text-green-600 bg-green-100';
  };

  const getStatusText = (plugin: PluginInfo) => {
    if (!plugin.isActive) return 'Inactive';
    return 'Active';
  };

  const getTypeIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'widget': return '🧩';
      case 'page': return '📄';
      case 'modal': return '🪟';
      case 'sidebar': return '📋';
      case 'toolbar': return '🔧';
      default: return '⚙️';
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
        <p className="text-center mt-4 text-gray-600">Loading plugins...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
        <button 
          onClick={loadPlugins}
          className="mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Plugin Management</h2>
          <p className="text-gray-600 mt-1">Manage installed plugins and their settings</p>
        </div>
        <div className="flex space-x-3">
          <button
            onClick={loadPlugins}
            className="px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600"
          >
            Refresh
          </button>
          <button
            onClick={handleReloadPlugins}
            className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          >
            Reload Plugins
          </button>
        </div>
      </div>

      {/* Plugin Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-blue-600">{plugins.length}</div>
          <div className="text-sm text-gray-600">Total Plugins</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-green-600">
            {plugins.filter(p => p.isActive).length}
          </div>
          <div className="text-sm text-gray-600">Active Plugins</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-orange-600">
            {plugins.reduce((sum, p) => sum + p.frontendComponents.length, 0)}
          </div>
          <div className="text-sm text-gray-600">Frontend Components</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-purple-600">
            {pluginRegistry.getAvailableHooks().length}
          </div>
          <div className="text-sm text-gray-600">Available Hooks</div>
        </div>
      </div>

      {/* Plugin List */}
      <div className="bg-white shadow rounded-lg">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-medium text-gray-900">Installed Plugins</h3>
        </div>
        
        {plugins.length === 0 ? (
          <div className="p-6 text-center text-gray-500">
            No plugins installed
          </div>
        ) : (
          <div className="divide-y divide-gray-200">
            {plugins.map((plugin) => (
              <div key={plugin.id} className="p-6 hover:bg-gray-50">
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3">
                      <h4 className="text-lg font-medium text-gray-900">{plugin.name}</h4>
                      <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(plugin)}`}>
                        {getStatusText(plugin)}
                      </span>
                    </div>
                    <p className="text-sm text-gray-600 mt-1">{plugin.description}</p>
                    <div className="flex items-center space-x-4 mt-2 text-sm text-gray-500">
                      <span>Version: {plugin.version}</span>
                      <span>Author: {plugin.author}</span>
                      <span>Components: {plugin.frontendComponents.length}</span>
                    </div>
                  </div>
                  
                  <div className="flex items-center space-x-2">
                    <button
                      onClick={() => {
                        setSelectedPlugin(plugin);
                        setShowDetails(true);
                      }}
                      className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200"
                    >
                      Details
                    </button>
                    
                    {plugin.metadata.lifecycle.canDisable && (
                      <button
                        className={`px-3 py-1 text-sm rounded ${
                          plugin.isActive 
                            ? 'bg-red-100 text-red-700 hover:bg-red-200' 
                            : 'bg-green-100 text-green-700 hover:bg-green-200'
                        }`}
                      >
                        {plugin.isActive ? 'Disable' : 'Enable'}
                      </button>
                    )}
                  </div>
                </div>
                
                {/* Frontend Components Preview */}
                {plugin.frontendComponents.length > 0 && (
                  <div className="mt-4 pt-4 border-t border-gray-100">
                    <h5 className="text-sm font-medium text-gray-700 mb-2">Frontend Components:</h5>
                    <div className="flex flex-wrap gap-2">
                      {plugin.frontendComponents.map((component) => (
                        <div
                          key={component.id}
                          className="inline-flex items-center px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded"
                        >
                          <span className="mr-1">{getTypeIcon(component.type)}</span>
                          {component.name}
                          <span className="ml-1 text-gray-500">
                            ({component.hooks.length} hook{component.hooks.length !== 1 ? 's' : ''})
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Plugin Details Modal */}
      {showDetails && selectedPlugin && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full mx-4 max-h-screen overflow-y-auto">
            <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
              <h3 className="text-lg font-medium text-gray-900">
                Plugin Details: {selectedPlugin.name}
              </h3>
              <button
                onClick={() => setShowDetails(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            <div className="p-6 space-y-6">
              {/* Basic Info */}
              <div>
                <h4 className="text-md font-semibold text-gray-900 mb-3">Basic Information</h4>
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="font-medium text-gray-700">ID:</span>
                    <span className="ml-2 text-gray-600">{selectedPlugin.id}</span>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">Version:</span>
                    <span className="ml-2 text-gray-600">{selectedPlugin.version}</span>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">Author:</span>
                    <span className="ml-2 text-gray-600">{selectedPlugin.author}</span>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">Status:</span>
                    <span className={`ml-2 px-2 py-1 text-xs rounded ${getStatusColor(selectedPlugin)}`}>
                      {getStatusText(selectedPlugin)}
                    </span>
                  </div>
                </div>
                <div className="mt-3">
                  <span className="font-medium text-gray-700">Description:</span>
                  <p className="mt-1 text-gray-600">{selectedPlugin.description}</p>
                </div>
              </div>

              {/* Permissions */}
              <div>
                <h4 className="text-md font-semibold text-gray-900 mb-3">Permissions</h4>
                <div className="space-y-3">
                  {selectedPlugin.metadata.requiredPermissions.length > 0 && (
                    <div>
                      <span className="font-medium text-gray-700">Required:</span>
                      <div className="mt-1 flex flex-wrap gap-1">
                        {selectedPlugin.metadata.requiredPermissions.map((perm) => (
                          <span key={perm} className="px-2 py-1 bg-red-100 text-red-800 text-xs rounded">
                            {perm}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}
                  {selectedPlugin.metadata.optionalPermissions.length > 0 && (
                    <div>
                      <span className="font-medium text-gray-700">Optional:</span>
                      <div className="mt-1 flex flex-wrap gap-1">
                        {selectedPlugin.metadata.optionalPermissions.map((perm) => (
                          <span key={perm} className="px-2 py-1 bg-yellow-100 text-yellow-800 text-xs rounded">
                            {perm}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Frontend Components */}
              {selectedPlugin.frontendComponents.length > 0 && (
                <div>
                  <h4 className="text-md font-semibold text-gray-900 mb-3">Frontend Components</h4>
                  <div className="space-y-4">
                    {selectedPlugin.frontendComponents.map((component) => (
                      <div key={component.id} className="border border-gray-200 rounded-lg p-4">
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex items-center space-x-2">
                            <span className="text-lg">{getTypeIcon(component.type)}</span>
                            <span className="font-medium text-gray-900">{component.name}</span>
                            <span className="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">
                              {component.type}
                            </span>
                          </div>
                        </div>
                        
                        {component.hooks.length > 0 && (
                          <div>
                            <span className="font-medium text-gray-700 text-sm">Hooks:</span>
                            <div className="mt-1 space-y-1">
                              {component.hooks.map((hook, index) => (
                                <div key={index} className="flex items-center justify-between text-sm">
                                  <span className="text-gray-600">{hook.hookName}</span>
                                  <span className="text-gray-500">Priority: {hook.priority}</span>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Lifecycle */}
              <div>
                <h4 className="text-md font-semibold text-gray-900 mb-3">Lifecycle Settings</h4>
                <div className="grid grid-cols-3 gap-4 text-sm">
                  <div className="flex items-center">
                    <span className={`w-3 h-3 rounded-full mr-2 ${
                      selectedPlugin.metadata.lifecycle.autoStart ? 'bg-green-500' : 'bg-gray-300'
                    }`}></span>
                    <span className="text-gray-700">Auto Start</span>
                  </div>
                  <div className="flex items-center">
                    <span className={`w-3 h-3 rounded-full mr-2 ${
                      selectedPlugin.metadata.lifecycle.canDisable ? 'bg-green-500' : 'bg-gray-300'
                    }`}></span>
                    <span className="text-gray-700">Can Disable</span>
                  </div>
                  <div className="flex items-center">
                    <span className={`w-3 h-3 rounded-full mr-2 ${
                      selectedPlugin.metadata.lifecycle.canUninstall ? 'bg-green-500' : 'bg-gray-300'
                    }`}></span>
                    <span className="text-gray-700">Can Uninstall</span>
                  </div>
                </div>
              </div>
            </div>
            
            <div className="px-6 py-4 border-t border-gray-200 flex justify-end space-x-3">
              <button
                onClick={() => setShowDetails(false)}
                className="px-4 py-2 text-gray-700 bg-gray-100 rounded hover:bg-gray-200"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PluginManagement; 