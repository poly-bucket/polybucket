import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import NavigationBar from '../../components/common/NavigationBar';
import { 
  Add as AddIcon,
  Download as DownloadIcon,
  Delete as DeleteIcon,
  Settings as SettingsIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon
} from '@mui/icons-material';

interface InstalledPlugin {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  type: string;
  status: string;
  installedAt: string;
  updatedAt?: string;
  installationPath: string;
  permissions: string[];
  isEnabled: boolean;
}

interface MarketplacePlugin {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  type: string;
  license: string;
  keywords: string[];
  downloadCount: number;
  rating: number;
  repositoryUrl: string;
  lastUpdated: string;
  isInstalled: boolean;
}

const PluginManagement: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<'installed' | 'marketplace' | 'install'>('installed');
  const [installedPlugins, setInstalledPlugins] = useState<InstalledPlugin[]>([]);
  const [marketplacePlugins, setMarketplacePlugins] = useState<MarketplacePlugin[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [installUrl, setInstallUrl] = useState('');
  const [installSource, setInstallSource] = useState<'url' | 'github' | 'marketplace'>('url');
  const [installing, setInstalling] = useState(false);

  useEffect(() => {
    loadInstalledPlugins();
    loadMarketplacePlugins();
  }, []);

  const loadInstalledPlugins = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/plugins/installed', {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });
      
      if (response.ok) {
        const plugins = await response.json();
        setInstalledPlugins(plugins);
      } else {
        setError('Failed to load installed plugins');
      }
    } catch (err) {
      setError('Error loading installed plugins');
      console.error('Error loading installed plugins:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadMarketplacePlugins = async () => {
    try {
      const response = await fetch('/api/plugins/marketplace', {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });
      
      if (response.ok) {
        const plugins = await response.json();
        setMarketplacePlugins(plugins);
      }
    } catch (err) {
      console.error('Error loading marketplace plugins:', err);
    }
  };

  const installPlugin = async () => {
    if (!installUrl.trim()) {
      setError('Please enter a URL or repository');
      return;
    }

    try {
      setInstalling(true);
      setError('');

      const requestBody: any = {
        source: installSource
      };

      if (installSource === 'url') {
        requestBody.url = installUrl;
      } else if (installSource === 'github') {
        requestBody.url = installUrl;
      } else if (installSource === 'marketplace') {
        requestBody.pluginId = installUrl;
      }

      const response = await fetch('/api/plugins/install', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${user?.accessToken}`
        },
        body: JSON.stringify(requestBody)
      });

      const result = await response.json();

      if (response.ok && result.success) {
        setInstallUrl('');
        await loadInstalledPlugins();
        setActiveTab('installed');
      } else {
        setError(result.message || 'Failed to install plugin');
      }
    } catch (err) {
      setError('Error installing plugin');
      console.error('Error installing plugin:', err);
    } finally {
      setInstalling(false);
    }
  };

  const uninstallPlugin = async (pluginId: string) => {
    if (!confirm(`Are you sure you want to uninstall ${pluginId}?`)) {
      return;
    }

    try {
      const response = await fetch(`/api/plugins/${pluginId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`
        }
      });

      if (response.ok) {
        await loadInstalledPlugins();
      } else {
        setError('Failed to uninstall plugin');
      }
    } catch (err) {
      setError('Error uninstalling plugin');
      console.error('Error uninstalling plugin:', err);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'active':
        return <CheckCircleIcon className="text-green-500" />;
      case 'error':
        return <ErrorIcon className="text-red-500" />;
      case 'inactive':
        return <WarningIcon className="text-yellow-500" />;
      default:
        return <WarningIcon className="text-gray-500" />;
    }
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'theme':
        return 'bg-purple-100 text-purple-800';
      case 'oauth':
        return 'bg-blue-100 text-blue-800';
      case 'metadata':
        return 'bg-green-100 text-green-800';
      case 'localization':
        return 'bg-yellow-100 text-yellow-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <NavigationBar />
        <div className="container mx-auto px-4 py-8">
          <div className="flex items-center justify-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
            <span className="ml-3">Loading plugins...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <NavigationBar />
      
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Plugin Management</h1>
          <p className="text-gray-600">Install, manage, and configure PolyBucket plugins</p>
        </div>

        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 rounded-md p-4">
            <div className="flex">
              <ErrorIcon className="h-5 w-5 text-red-400" />
              <div className="ml-3">
                <h3 className="text-sm font-medium text-red-800">Error</h3>
                <p className="text-sm text-red-700 mt-1">{error}</p>
              </div>
            </div>
          </div>
        )}

        {/* Tab Navigation */}
        <div className="mb-6">
          <nav className="flex space-x-8">
            <button
              onClick={() => setActiveTab('installed')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'installed'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Installed ({installedPlugins.length})
            </button>
            <button
              onClick={() => setActiveTab('marketplace')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'marketplace'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Marketplace
            </button>
            <button
              onClick={() => setActiveTab('install')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'install'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Install Plugin
            </button>
          </nav>
        </div>

        {/* Installed Plugins Tab */}
        {activeTab === 'installed' && (
          <div className="space-y-4">
            {installedPlugins.length === 0 ? (
              <div className="text-center py-12">
                <SettingsIcon className="mx-auto h-12 w-12 text-gray-400" />
                <h3 className="mt-2 text-sm font-medium text-gray-900">No plugins installed</h3>
                <p className="mt-1 text-sm text-gray-500">Get started by installing a plugin from the marketplace.</p>
                <div className="mt-6">
                  <button
                    onClick={() => setActiveTab('marketplace')}
                    className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                  >
                    <AddIcon className="h-4 w-4 mr-2" />
                    Browse Marketplace
                  </button>
                </div>
              </div>
            ) : (
              <div className="grid gap-4">
                {installedPlugins.map((plugin) => (
                  <div key={plugin.id} className="bg-white rounded-lg shadow p-6">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center space-x-3">
                          <h3 className="text-lg font-medium text-gray-900">{plugin.name}</h3>
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getTypeColor(plugin.type)}`}>
                            {plugin.type}
                          </span>
                          <div className="flex items-center space-x-1">
                            {getStatusIcon(plugin.status)}
                            <span className="text-sm text-gray-500 capitalize">{plugin.status}</span>
                          </div>
                        </div>
                        <p className="mt-1 text-sm text-gray-600">{plugin.description}</p>
                        <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                          <span>by {plugin.author}</span>
                          <span>v{plugin.version}</span>
                          <span>Installed {new Date(plugin.installedAt).toLocaleDateString()}</span>
                        </div>
                        {plugin.permissions.length > 0 && (
                          <div className="mt-2">
                            <span className="text-xs text-gray-500">Permissions: </span>
                            <span className="text-xs text-gray-700">{plugin.permissions.join(', ')}</span>
                          </div>
                        )}
                      </div>
                      <div className="flex items-center space-x-2">
                        <button
                          onClick={() => {/* TODO: Configure plugin */}}
                          className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                        >
                          <SettingsIcon className="h-4 w-4 mr-1" />
                          Configure
                        </button>
                        <button
                          onClick={() => uninstallPlugin(plugin.id)}
                          className="inline-flex items-center px-3 py-2 border border-red-300 shadow-sm text-sm leading-4 font-medium rounded-md text-red-700 bg-white hover:bg-red-50"
                        >
                          <DeleteIcon className="h-4 w-4 mr-1" />
                          Uninstall
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Marketplace Tab */}
        {activeTab === 'marketplace' && (
          <div className="space-y-4">
            {marketplacePlugins.map((plugin) => (
              <div key={plugin.id} className="bg-white rounded-lg shadow p-6">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3">
                      <h3 className="text-lg font-medium text-gray-900">{plugin.name}</h3>
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getTypeColor(plugin.type)}`}>
                        {plugin.type}
                      </span>
                      {plugin.isInstalled && (
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                          Installed
                        </span>
                      )}
                    </div>
                    <p className="mt-1 text-sm text-gray-600">{plugin.description}</p>
                    <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                      <span>by {plugin.author}</span>
                      <span>v{plugin.version}</span>
                      <span>{plugin.downloadCount} downloads</span>
                      <span>★ {plugin.rating}</span>
                    </div>
                    <div className="mt-2 flex flex-wrap gap-1">
                      {plugin.keywords.map((keyword) => (
                        <span key={keyword} className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-gray-100 text-gray-800">
                          {keyword}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div className="flex items-center space-x-2">
                    {!plugin.isInstalled ? (
                      <button
                        onClick={() => {
                          setInstallUrl(plugin.id);
                          setInstallSource('marketplace');
                          setActiveTab('install');
                        }}
                        className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                      >
                        <DownloadIcon className="h-4 w-4 mr-2" />
                        Install
                      </button>
                    ) : (
                      <span className="text-sm text-gray-500">Already installed</span>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Install Plugin Tab */}
        {activeTab === 'install' && (
          <div className="max-w-2xl">
            <div className="bg-white rounded-lg shadow p-6">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Install Plugin</h3>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Installation Source
                  </label>
                  <select
                    value={installSource}
                    onChange={(e) => setInstallSource(e.target.value as any)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="url">Direct URL</option>
                    <option value="github">GitHub Repository</option>
                    <option value="marketplace">Marketplace</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    {installSource === 'url' && 'Plugin URL'}
                    {installSource === 'github' && 'GitHub Repository URL'}
                    {installSource === 'marketplace' && 'Plugin ID'}
                  </label>
                  <input
                    type="text"
                    value={installUrl}
                    onChange={(e) => setInstallUrl(e.target.value)}
                    placeholder={
                      installSource === 'url' ? 'https://example.com/plugin.zip' :
                      installSource === 'github' ? 'https://github.com/user/repo' :
                      'plugin-id'
                    }
                    className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                <div className="flex items-center space-x-3">
                  <button
                    onClick={installPlugin}
                    disabled={installing || !installUrl.trim()}
                    className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {installing ? (
                      <>
                        <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                        Installing...
                      </>
                    ) : (
                      <>
                        <DownloadIcon className="h-4 w-4 mr-2" />
                        Install Plugin
                      </>
                    )}
                  </button>
                  <button
                    onClick={() => setInstallUrl('')}
                    className="px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                  >
                    Clear
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default PluginManagement;
