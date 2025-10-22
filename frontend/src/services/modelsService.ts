import store from '../store';
import { PrivacySettings } from './api.client';
import { ApiClientFactory } from '../api/clientFactory';
import SearchService from './searchService';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:11666';

// Helper function to get auth token from Redux store
const getAuthToken = (): string | null => {
  const state = store.getState();
  return state.auth.user?.accessToken || null;
};

export interface ModelUploadData {
  name: string;
  description?: string;
  privacy: PrivacySettings;
  license?: string;
  categories?: string[];
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
  thumbnailFileId?: string;
}

export interface ModelUploadRequest {
  modelData: ModelUploadData;
  files: File[];
}

export interface Model {
  id: string;
  name: string;
  description: string;
  userId: string;
  license?: string;
  privacy: PrivacySettings;
  categories?: string[];
  aiGenerated: boolean;
  wip: boolean;
  nsfw: boolean;
  isRemix: boolean;
  remixUrl?: string;
  author?: any;
  files?: any[];
  isPublic: boolean;
  isFeatured: boolean;
  categoryCollection?: any[];
  tags?: any[];
  versions?: any[];
  comments?: any[];
  likes?: any[];
  authorId: string;
  createdAt: string;
  updatedAt?: string;
  thumbnailUrl?: string;
  fileUrl?: string;
  downloads?: number;
}

export interface GetModelsResponse {
  models: Model[];
  totalCount: number;
  page: number;
  totalPages: number;
}

export interface GetModelByIdResponse {
  model: Model;
}

export interface ExtendedModel extends Model {
  // Additional properties for the frontend
  downloadCount?: number;
  rating?: number;
  isLiked?: boolean;
  isInCollection?: boolean;
}

export class ModelsService {
  static async uploadModel(request: ModelUploadRequest): Promise<Model> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const formData = new FormData();
    
    // Add model data
    formData.append('name', request.modelData.name);
    formData.append('description', request.modelData.description || '');
    formData.append('privacy', request.modelData.privacy.toString());
    formData.append('license', request.modelData.license || 'MIT');
    formData.append('categories', JSON.stringify(request.modelData.categories || []));
    formData.append('aiGenerated', request.modelData.aiGenerated.toString());
    formData.append('workInProgress', request.modelData.workInProgress.toString());
    formData.append('nsfw', request.modelData.nsfw.toString());
    formData.append('remix', request.modelData.remix.toString());

    // Add files
    request.files.forEach((file) => {
      formData.append('files', file);
    });

    // Add thumbnail file ID if specified
    if (request.modelData.thumbnailFileId) {
      formData.append('thumbnailFileId', request.modelData.thumbnailFileId);
    }

    const response = await fetch(`${API_BASE_URL}/api/models`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      },
      body: formData
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Upload failed: ${response.status} ${response.statusText} - ${errorText}`);
    }

    const result = await response.json();
    return result;
  }

  static async getModels(page?: number, take?: number): Promise<GetModelsResponse> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const params = new URLSearchParams();
    if (page !== undefined) params.append('page', page.toString());
    if (take !== undefined) params.append('take', take.toString());

    const response = await fetch(`${API_BASE_URL}/api/models?${params}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/json'
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to get models: ${response.status} ${response.statusText} - ${errorText}`);
    }

    return await response.json();
  }

  static async getModelById(id: string): Promise<GetModelByIdResponse> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const response = await fetch(`${API_BASE_URL}/api/models/${id}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/json'
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to get model: ${response.status} ${response.statusText} - ${errorText}`);
    }

    return await response.json();
  }

  static async deleteModel(id: string): Promise<void> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const response = await fetch(`${API_BASE_URL}/api/models/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to delete model: ${response.status} ${response.statusText} - ${errorText}`);
    }
  }

  static async updateModel(id: string, modelData: Partial<Model>): Promise<void> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const response = await fetch(`${API_BASE_URL}/api/models/${id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(modelData)
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Update failed: ${response.status} ${response.statusText} - ${errorText}`);
    }
  }

  static async createModelVersion(modelId: string, versionData: { name: string; files: File[] }): Promise<void> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const formData = new FormData();
    formData.append('name', versionData.name);
    versionData.files.forEach((file) => {
      formData.append('files', file);
    });

    const response = await fetch(`${API_BASE_URL}/api/models/${modelId}/versions`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      },
      body: formData
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Version creation failed: ${response.status} ${response.statusText} - ${errorText}`);
    }
  }

  static async getModelVersions(modelId: string): Promise<any> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const response = await fetch(`${API_BASE_URL}/api/models/${modelId}/versions`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/json'
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to get model versions: ${response.status} ${response.statusText} - ${errorText}`);
    }

    return await response.json();
  }

  // Dashboard methods for different model categories
  static async getFeaturedModels(): Promise<ExtendedModel[]> {
    try {
      // Use the generated API client instead of direct fetch
      const client = ApiClientFactory.getModelsClient();
      const response = await client.getModels(1, 50);
      
      // Filter for featured models
      const featuredModels = response.models?.filter((model: any) => model.isFeatured) || [];
      
      return featuredModels.map((model: any) => ({
        ...model,
        // The backend already provides thumbnailUrl correctly, don't override it
        downloadCount: model.downloads || 0,
        rating: 0, // TODO: Implement rating system
        isLiked: false, // TODO: Check if current user liked this model
        isInCollection: false // TODO: Check if model is in user's collections
      }));
    } catch (error) {
      console.error('Error getting featured models:', error);
      throw new Error('Failed to get featured models');
    }
  }

  static async getPopularModels(): Promise<ExtendedModel[]> {
    try {
      // Use the generated API client instead of direct fetch
      const client = ApiClientFactory.getModelsClient();
      const response = await client.getModels(1, 50);
      
      // Sort by downloads + likes (popularity score)
      const popularModels = (response.models || [])
        .sort((a: any, b: any) => {
          const scoreA = (a.downloads || 0) + (a.likes || 0);
          const scoreB = (b.downloads || 0) + (b.likes || 0);
          return scoreB - scoreA;
        })
        .slice(0, 20); // Take top 20
      
      return popularModels.map((model: any) => ({
        ...model,
        // The backend already provides thumbnailUrl correctly, don't override it
        downloadCount: model.downloads || 0,
        rating: 0, // TODO: Implement rating system
        isLiked: false, // TODO: Check if current user liked this model
        isInCollection: false // TODO: Check if model is in user's collections
      }));
    } catch (error) {
      console.error('Error getting popular models:', error);
      throw new Error('Failed to get popular models');
    }
  }

  static async getRecentModels(): Promise<ExtendedModel[]> {
    try {
      // Use the generated API client instead of direct fetch
      const client = ApiClientFactory.getModelsClient();
      const response = await client.getModels(1, 50);
      
      // Sort by creation date (most recent first)
      const recentModels = (response.models || [])
        .sort((a: any, b: any) => {
          const dateA = new Date(a.createdAt || 0);
          const dateB = new Date(b.createdAt || 0);
          return dateB.getTime() - dateA.getTime();
        })
        .slice(0, 20); // Take top 20
      
      return recentModels.map((model: any) => ({
        ...model,
        // The backend already provides thumbnailUrl correctly, don't override it
        downloadCount: model.downloads || 0,
        rating: 0, // TODO: Implement rating system
        isLiked: false, // TODO: Check if current user liked this model
        isInCollection: false // TODO: Check if model is in user's collections
      }));
    } catch (error) {
      console.error('Error getting recent models:', error);
      throw new Error('Failed to get recent models');
    }
  }

  static async searchModels(params: { searchQuery: string }): Promise<{ models: ExtendedModel[] }> {
    try {
      // Use the new comprehensive search service
      return await SearchService.searchModels(params);
    } catch (error) {
      console.error('Error searching models:', error);
      throw new Error('Failed to search models');
    }
  }
}

// Create a default export object with all the service methods
const modelsService = {
  uploadModel: ModelsService.uploadModel,
  getModels: ModelsService.getModels,
  getModelById: ModelsService.getModelById,
  deleteModel: ModelsService.deleteModel,
  updateModel: ModelsService.updateModel,
  createModelVersion: ModelsService.createModelVersion,
  getModelVersions: ModelsService.getModelVersions,
  getFeaturedModels: ModelsService.getFeaturedModels,
  getPopularModels: ModelsService.getPopularModels,
  getRecentModels: ModelsService.getRecentModels,
  searchModels: ModelsService.searchModels
};

export default modelsService; 