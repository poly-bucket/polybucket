import { 
  PluginSummary, 
  PluginBrowseRequest, 
  PluginBrowseResponse, 
  PluginCategory,
  PluginDownloadInfo,
  MarketplacePluginDetails,
  PluginInstallationRequest,
  PluginInstallationResponse
} from '@/types/plugin';
import { API_BASE_URL } from './api-config';

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    
    const response = await fetch(url, {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  // Plugin browsing endpoints
  async browsePlugins(request: PluginBrowseRequest): Promise<PluginBrowseResponse> {
    return this.request<PluginBrowseResponse>('/api/plugins/browse', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async getFeaturedPlugins(limit: number = 6): Promise<PluginSummary[]> {
    return this.request<PluginSummary[]>(`/api/plugins/featured?limit=${limit}`);
  }

  async getTrendingPlugins(limit: number = 10): Promise<PluginSummary[]> {
    return this.request<PluginSummary[]>(`/api/plugins/trending?limit=${limit}`);
  }

  async getPopularTags(limit: number = 20): Promise<string[]> {
    return this.request<string[]>(`/api/plugins/tags/popular?limit=${limit}`);
  }

  async getCategories(): Promise<string[]> {
    return this.request<string[]>('/api/plugins/categories');
  }

  async getPlugin(id: string): Promise<PluginSummary> {
    return this.request<PluginSummary>(`/api/plugins/${id}`);
  }

  // Plugin installation endpoints
  async getPluginDownload(pluginId: string, version?: string): Promise<PluginDownloadInfo> {
    const params = version ? `?version=${version}` : '';
    return this.request<PluginDownloadInfo>(`/api/plugins/${pluginId}/download${params}`);
  }

  async getPluginDetails(pluginId: string): Promise<MarketplacePluginDetails> {
    return this.request<MarketplacePluginDetails>(`/api/plugins/${pluginId}/details`);
  }

  async recordInstallation(request: PluginInstallationRequest): Promise<PluginInstallationResponse> {
    return this.request<PluginInstallationResponse>('/api/plugins/install', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  // Direct repository access (for offline resilience)
  async getRepositoryInfo(repositoryUrl: string): Promise<{
    name: string;
    description: string;
    stars: number;
    forks: number;
    lastCommit: string;
    license?: string;
    readme?: string;
  }> {
    // This would typically call a service that fetches from GitHub/GitLab APIs
    // For now, we'll return a mock response
    return this.request(`/api/repository/info?url=${encodeURIComponent(repositoryUrl)}`);
  }
}

export const apiClient = new ApiClient();
