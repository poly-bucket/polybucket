import React, { useState, useEffect } from 'react';
import { Extension as ExtensionIcon, Store as StoreIcon, Search as SearchIcon, Download as DownloadIcon } from '@mui/icons-material';

interface MarketplacePlugin {
  id: string;
  name: string;
  description: string;
  version: string;
  author: string;
  category: string;
  downloads: number;
  rating: number;
  reviewCount: number;
  isVerified: boolean;
  isFeatured: boolean;
  lastUpdated: string;
  license: string;
  screenshots: string[];
  documentationUrl: string;
}

const PluginsTab: React.FC = () => {
  const [plugins, setPlugins] = useState([
    { id: 'comments', name: 'Comments Plugin', status: 'enabled', version: '1.0.0' },
    { id: 'federation', name: 'Federation Plugin', status: 'disabled', version: '1.2.0' },
    { id: 'analytics', name: 'Analytics Plugin', status: 'enabled', version: '2.1.0' },
  ]);

  const [marketplacePlugins, setMarketplacePlugins] = useState<MarketplacePlugin[]>([]);
  const [marketplaceLoading, setMarketplaceLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [categories, setCategories] = useState<string[]>([]);
  const [activeTab, setActiveTab] = useState<'installed' | 'marketplace'>('installed');

  const [selectedPlugin, setSelectedPlugin] = useState<string | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [hooksOpen, setHooksOpen] = useState(false);

  const loadPluginData = async () => {
    // Mock API call
    console.log('Loading plugin data...');
  };

  const loadMarketplacePlugins = async () => {
    try {
      setMarketplaceLoading(true);
      const response = await fetch('/api/plugins/marketplace', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setMarketplacePlugins(data.plugins || []);
      }
    } catch (error) {
      console.error('Error loading marketplace plugins:', error);
    } finally {
      setMarketplaceLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await fetch('/api/plugins/marketplace/categories', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setCategories(data.map((cat: any) => cat.name));
      }
    } catch (error) {
      console.error('Error loading categories:', error);
    }
  };

  const searchMarketplace = async () => {
    try {
      setMarketplaceLoading(true);
      const params = new URLSearchParams();
      if (searchQuery) params.append('query', searchQuery);
      if (selectedCategory) params.append('category', selectedCategory);
      
      const response = await fetch(`/api/plugins/marketplace/search?${params}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setMarketplacePlugins(data.plugins || []);
      }
    } catch (error) {
      console.error('Error searching marketplace:', error);
    } finally {
      setMarketplaceLoading(false);
    }
  };

  const installFromMarketplace = async (pluginId: string) => {
    try {
      const response = await fetch(`/api/plugins/marketplace/${pluginId}/install`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
          'Content-Type': 'application/json'
        }
      });
      
      if (response.ok) {
        console.log(`Plugin ${pluginId} installed successfully`);
        // Refresh installed plugins
        loadPluginData();
      } else {
        console.error('Failed to install plugin');
      }
    } catch (error) {
      console.error('Error installing plugin:', error);
    }
  };

  useEffect(() => {
    loadPluginData();
    loadCategories();
  }, []);

  useEffect(() => {
    if (activeTab === 'marketplace') {
      loadMarketplacePlugins();
    }
  }, [activeTab]);

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
        <div className="flex space-x-3">
          <button
            onClick={handleReloadPlugins}
            className="lg-button lg-button-secondary"
          >
            Reload Plugins
          </button>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="flex space-x-1 bg-white/5 rounded-lg p-1">
        <button
          onClick={() => setActiveTab('installed')}
          className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
            activeTab === 'installed'
              ? 'bg-indigo-500 text-white'
              : 'text-white/60 hover:text-white hover:bg-white/10'
          }`}
        >
          <ExtensionIcon className="w-4 h-4 mr-2 inline" />
          Installed Plugins
        </button>
        <button
          onClick={() => setActiveTab('marketplace')}
          className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
            activeTab === 'marketplace'
              ? 'bg-indigo-500 text-white'
              : 'text-white/60 hover:text-white hover:bg-white/10'
          }`}
        >
          <StoreIcon className="w-4 h-4 mr-2 inline" />
          Marketplace
        </button>
      </div>

      {/* Installed Plugins Tab */}
      {activeTab === 'installed' && (
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
      )}

      {/* Marketplace Tab */}
      {activeTab === 'marketplace' && (
        <div className="space-y-6">
          {/* Search and Filters */}
          <div className="lg-card p-6">
            <div className="flex flex-col md:flex-row gap-4">
              <div className="flex-1">
                <div className="relative">
                  <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/40 w-5 h-5" />
                  <input
                    type="text"
                    placeholder="Search plugins..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="lg-input pl-10"
                  />
                </div>
              </div>
              <div className="md:w-48">
                <select
                  value={selectedCategory}
                  onChange={(e) => setSelectedCategory(e.target.value)}
                  className="lg-input"
                >
                  <option value="">All Categories</option>
                  {categories.map((category) => (
                    <option key={category} value={category}>{category}</option>
                  ))}
                </select>
              </div>
              <button
                onClick={searchMarketplace}
                className="lg-button lg-button-primary"
              >
                Search
              </button>
            </div>
          </div>

          {/* Marketplace Plugins */}
          <div className="lg-card p-6">
            <h3 className="text-lg font-medium text-white mb-4">Available Plugins</h3>
            
            {marketplaceLoading ? (
              <div className="flex justify-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-500"></div>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {marketplacePlugins.map((plugin) => (
                  <div key={plugin.id} className="bg-white/5 rounded-lg p-4 hover:bg-white/10 transition-colors">
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex items-center space-x-3">
                        <div className="w-10 h-10 bg-indigo-500 rounded flex items-center justify-center">
                          <ExtensionIcon className="w-6 h-6 text-white" />
                        </div>
                        <div>
                          <div className="text-white font-medium">{plugin.name}</div>
                          <div className="text-sm text-white/60">by {plugin.author}</div>
                        </div>
                      </div>
                      {plugin.isVerified && (
                        <span className="lg-badge lg-badge-success text-xs">Verified</span>
                      )}
                    </div>
                    
                    <p className="text-white/80 text-sm mb-3 line-clamp-2">{plugin.description}</p>
                    
                    <div className="flex items-center justify-between text-sm text-white/60 mb-3">
                      <div className="flex items-center space-x-4">
                        <span>{plugin.downloads.toLocaleString()} downloads</span>
                        <span>★ {plugin.rating.toFixed(1)} ({plugin.reviewCount})</span>
                      </div>
                      <span className="text-xs bg-white/10 px-2 py-1 rounded">{plugin.category}</span>
                    </div>
                    
                    <div className="flex items-center justify-between">
                      <span className="text-xs text-white/60">v{plugin.version}</span>
                      <button
                        onClick={() => installFromMarketplace(plugin.id)}
                        className="lg-button lg-button-primary text-sm"
                      >
                        <DownloadIcon className="w-4 h-4 mr-1" />
                        Install
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

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
