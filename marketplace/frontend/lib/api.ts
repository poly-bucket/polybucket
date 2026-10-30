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

  async createSubmission(payload: {
    name: string;
    description: string;
    category: string;
    license: string;
    repositoryUrl: string;
    repositoryBranch?: string;
    repositorySubpath?: string;
  }): Promise<SubmissionSummary> {
    return this.request<SubmissionSummary>('/api/submissions', {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(payload),
    });
  }

  async getSubmissions(status: string = 'pending'): Promise<SubmissionSummary[]> {
    return this.request<SubmissionSummary[]>(`/api/submissions?status=${encodeURIComponent(status)}`, {
      headers: this.getAuthHeaders(),
    });
  }

  async approveSubmission(id: string): Promise<{ success: boolean; message: string; pluginId?: string }> {
    return this.request(`/api/submissions/${encodeURIComponent(id)}/approve`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
    });
  }

  async rejectSubmission(id: string, reviewNotes: string): Promise<SubmissionSummary> {
    return this.request<SubmissionSummary>(`/api/submissions/${encodeURIComponent(id)}/reject`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ reviewNotes }),
    });
  }

  async getPublicProfile(username: string): Promise<PublicUserProfile> {
    return this.request<PublicUserProfile>(`/api/users/${encodeURIComponent(username)}`);
  }

  async getPluginDownloadStats(pluginId: string, days = 30): Promise<PluginDownloadStat[]> {
    return this.request<PluginDownloadStat[]>(
      `/api/plugins/${encodeURIComponent(pluginId)}/downloads?days=${days}`
    );
  }

  async setPluginFeatured(pluginId: string, isFeatured: boolean): Promise<void> {
    await this.request(`/api/plugins/${encodeURIComponent(pluginId)}/featured`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ isFeatured }),
    });
  }

  async getPluginReviews(pluginId: string, page = 1, pageSize = 20): Promise<PluginReviewsResponse> {
    return this.request<PluginReviewsResponse>(
      `/api/plugins/${encodeURIComponent(pluginId)}/reviews?page=${page}&pageSize=${pageSize}`
    );
  }

  async createPluginReview(
    pluginId: string,
    payload: { rating: number; title: string; content: string }
  ): Promise<PluginReviewItem> {
    return this.request<PluginReviewItem>(`/api/plugins/${encodeURIComponent(pluginId)}/reviews`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(payload),
    });
  }

  async updatePluginReview(
    pluginId: string,
    reviewId: string,
    payload: { rating: number; title: string; content: string }
  ): Promise<PluginReviewItem> {
    return this.request<PluginReviewItem>(
      `/api/plugins/${encodeURIComponent(pluginId)}/reviews/${encodeURIComponent(reviewId)}`,
      {
        method: 'PUT',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(payload),
      }
    );
  }

  async deletePluginReview(pluginId: string, reviewId: string): Promise<void> {
    await this.request(`/api/plugins/${encodeURIComponent(pluginId)}/reviews/${encodeURIComponent(reviewId)}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
    });
  }

  async getDeveloperStats(): Promise<DeveloperStats> {
    return this.request<DeveloperStats>('/api/developer/stats', {
      headers: this.getAuthHeaders(),
    });
  }

  async getDeveloperPlugins(): Promise<DeveloperPluginSummary[]> {
    return this.request<DeveloperPluginSummary[]>('/api/developer/plugins', {
      headers: this.getAuthHeaders(),
    });
  }

  async getDeveloperAnalytics(): Promise<DeveloperAnalytics> {
    return this.request<DeveloperAnalytics>('/api/developer/analytics', {
      headers: this.getAuthHeaders(),
    });
  }

  private getAuthHeaders(): Record<string, string> {
    if (typeof window === 'undefined') {
      return {};
    }
    const token = localStorage.getItem('auth_token');
    return token ? { Authorization: `Bearer ${token}` } : {};
  }
}

export const apiClient = new ApiClient();

export interface SubmissionSummary {
  id: string;
  name: string;
  description: string;
  category: string;
  repositoryUrl: string;
  repositoryBranch: string;
  repositorySubpath: string;
  license: string;
  status: string;
  reviewNotes?: string;
  submittedAt: string;
  reviewedAt?: string;
}

export interface PluginDownloadStat {
  date: string;
  count: number;
}

export interface PublicUserProfile {
  username: string;
  displayName?: string;
  avatarUrl?: string;
  bio?: string;
  githubProfileUrl?: string;
  website?: string;
  createdAt: string;
  pluginCount: number;
  totalDownloads: number;
  plugins: PluginSummary[];
}

export interface PluginReviewItem {
  id: string;
  userId: string;
  username: string;
  rating: number;
  title: string;
  content: string;
  createdAt: string;
  isVerified: boolean;
  helpfulCount: number;
}

export interface PluginReviewsResponse {
  reviews: PluginReviewItem[];
  totalReviews: number;
  averageRating: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DeveloperStats {
  totalPlugins: number;
  totalDownloads: number;
  averageRating: number;
  totalReviews: number;
  monthlyDownloads: number;
  topPlugin?: { id: string; name: string; downloads: number };
}

export interface DeveloperPluginSummary {
  id: string;
  name: string;
  description: string;
  version: string;
  downloads: number;
  rating: number;
  reviewCount: number;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface DeveloperAnalytics {
  downloads: { date: string; count: number }[];
  topPlugins: { name: string; downloads: number }[];
}
