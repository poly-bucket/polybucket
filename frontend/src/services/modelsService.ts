import { 
  GetModelsQueryClient, 
  GetModelByIdQueryClient, 
  SearchModelsClient,
  GetModelsResponse,
  GetModelByIdResponse,
  Model 
} from './api.client';
import { 
  getFeaturedDemoModels, 
  getPopularDemoModels, 
  getRecentDemoModels, 
  searchDemoModels,
  demoModels 
} from './demoData';

// Extended model interface to include missing properties from backend
export interface ExtendedModel extends Model {
  thumbnailUrl?: string;
  downloads?: number;
}

export interface ModelsQueryParams {
  page?: number;
  pageSize?: number;
  searchQuery?: string;
  category?: string;
  sortBy?: 'popular' | 'newest' | 'downloads' | 'likes';
  showWIP?: boolean;
  showNSFW?: boolean;
  showAI?: boolean;
}

export interface ModelsResponse {
  models: ExtendedModel[];
  totalCount: number;
  page: number;
  totalPages: number;
  hasMore: boolean;
}

class ModelsService {
  private getModelsClient: GetModelsQueryClient;
  private getModelByIdClient: GetModelByIdQueryClient;
  private searchModelsClient: SearchModelsClient;

  constructor() {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
    this.getModelsClient = new GetModelsQueryClient(baseUrl);
    this.getModelByIdClient = new GetModelByIdQueryClient(baseUrl);
    this.searchModelsClient = new SearchModelsClient(baseUrl);
  }

  async getModels(params: ModelsQueryParams = {}): Promise<ModelsResponse> {
    try {
      const { page = 1, pageSize = 20 } = params;
      
      // For demo purposes, use demo data instead of API
      // TODO: Replace with actual API call when backend is ready
      const startIndex = (page - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      const paginatedModels = demoModels.slice(startIndex, endIndex);
      
      return {
        models: paginatedModels,
        totalCount: demoModels.length,
        page,
        totalPages: Math.ceil(demoModels.length / pageSize),
        hasMore: endIndex < demoModels.length
      };
    } catch (error) {
      console.error('Error fetching models:', error);
      throw new Error('Failed to fetch models. Please try again later.');
    }
  }

  async getModelById(id: string): Promise<ExtendedModel | null> {
    try {
      // Using demo data for now
      // TODO: Replace with actual API call when backend is ready
      const model = demoModels.find(m => m.id === id);
      return model || null;
    } catch (error) {
      console.error('Error fetching model by ID:', error);
      throw new Error('Failed to fetch model details. Please try again later.');
    }
  }

  async searchModels(params: ModelsQueryParams): Promise<ModelsResponse> {
    try {
      const { searchQuery = '', page = 1, pageSize = 20 } = params;
      
      // Using demo data for search
      let filteredModels = searchQuery ? searchDemoModels(searchQuery) : demoModels;

      // Filter by flags
      if (params.showWIP === false) {
        filteredModels = filteredModels.filter(model => !model.wip);
      }
      if (params.showNSFW === false) {
        filteredModels = filteredModels.filter(model => !model.nsfw);
      }
      if (params.showAI === false) {
        filteredModels = filteredModels.filter(model => !model.aiGenerated);
      }

      // Simple sorting
      if (params.sortBy) {
        filteredModels.sort((a, b) => {
          switch (params.sortBy) {
            case 'popular':
              return (b.likes?.length || 0) - (a.likes?.length || 0);
            case 'downloads':
              return (b.downloads || 0) - (a.downloads || 0);
            case 'newest':
              return new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime();
            default:
              return 0;
          }
        });
      }

      // Pagination
      const startIndex = (page - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      const paginatedModels = filteredModels.slice(startIndex, endIndex);
      
      return {
        models: paginatedModels,
        totalCount: filteredModels.length,
        page,
        totalPages: Math.ceil(filteredModels.length / pageSize),
        hasMore: endIndex < filteredModels.length
      };
    } catch (error) {
      console.error('Error searching models:', error);
      throw new Error('Failed to search models. Please try again later.');
    }
  }

  async getFeaturedModels(): Promise<ExtendedModel[]> {
    try {
      // Using demo data for now
      return getFeaturedDemoModels();
    } catch (error) {
      console.error('Error fetching featured models:', error);
      return [];
    }
  }

  async getPopularModels(): Promise<ExtendedModel[]> {
    try {
      // Using demo data for now
      return getPopularDemoModels();
    } catch (error) {
      console.error('Error fetching popular models:', error);
      return [];
    }
  }

  async getRecentModels(): Promise<ExtendedModel[]> {
    try {
      // Using demo data for now
      return getRecentDemoModels();
    } catch (error) {
      console.error('Error fetching recent models:', error);
      return [];
    }
  }

  // Utility methods for formatting
  formatDownloadCount(downloads: number): string {
    if (downloads >= 1000000) {
      return (downloads / 1000000).toFixed(1) + 'M';
    }
    if (downloads >= 1000) {
      return (downloads / 1000).toFixed(1) + 'K';
    }
    return downloads.toString();
  }

  formatLikeCount(likes: number): string {
    return this.formatDownloadCount(likes);
  }

  getModelThumbnail(model: ExtendedModel): string {
    return model.thumbnailUrl || 'https://images.unsplash.com/photo-1581833971358-2c8b550f87b3?w=400&h=225&fit=crop';
  }

  getModelUrl(model: ExtendedModel): string {
    return `/models/${model.id}`;
  }
}

// Export a singleton instance
export const modelsService = new ModelsService();
export default modelsService; 