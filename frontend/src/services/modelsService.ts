import store from '../store';
import { ApiClientFactory } from '../api/clientFactory';
import SearchService from './searchService';
import { API_CONFIG } from '../api/config';
import { PrivacySettings, UpdateModelRequest } from '../api/client';

export interface FileParameter {
  data: File;
  fileName: string;
}

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
  isFederated?: boolean;
  remoteInstanceId?: string;
  remoteModelId?: string;
  remoteAuthorId?: string;
  lastFederationSync?: string;
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
    const user = store.getState().auth.user;
    if (!user?.accessToken) {
      throw new Error('Authentication token not found');
    }

    const formData = new FormData();
    
    formData.append('Name', request.modelData.name);
    
    if (request.modelData.description) {
      formData.append('Description', request.modelData.description);
    }
    
    request.files.forEach((file) => {
      formData.append('Files', file);
    });
    
    if (request.modelData.thumbnailFileId) {
      formData.append('ThumbnailFileId', request.modelData.thumbnailFileId);
    }
    
    if (request.modelData.privacy !== undefined) {
      formData.append('Privacy', request.modelData.privacy.toString().toLowerCase());
    }
    
    if (request.modelData.license) {
      formData.append('License', request.modelData.license);
    }
    
    formData.append('AIGenerated', request.modelData.aiGenerated.toString());
    formData.append('WorkInProgress', request.modelData.workInProgress.toString());
    formData.append('NSFW', request.modelData.nsfw.toString());
    formData.append('Remix', request.modelData.remix.toString());

    const response = await fetch(`${API_CONFIG.baseUrl}/api/models`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${user.accessToken}`
      },
      body: formData
    });

    if (!response.ok) {
      let errorMessage = `Upload failed: ${response.statusText}`;
      try {
        const errorText = await response.text();
        if (errorText) {
          try {
            const errorData = JSON.parse(errorText);
            errorMessage = errorData.message || errorData.detail || errorData.title || errorText;
          } catch {
            errorMessage = errorText;
          }
        }
      } catch {
        // If we can't read the response, use the status text
      }
      const error = new Error(errorMessage);
      (error as any).status = response.status;
      throw error;
    }

    const result = await response.json();
    return result.model || result as Model;
  }

  static async getModels(page?: number, take?: number): Promise<GetModelsResponse> {
    const response = await ApiClientFactory.getApiClient().getModels_GetModels(page || 1, take || 20);
    return {
      models: response.models || [],
      totalCount: response.totalCount || 0,
      page: response.page || page || 1,
      totalPages: response.totalPages || 0
    };
  }

  static async getModelById(id: string): Promise<GetModelByIdResponse> {
    const response = await ApiClientFactory.getApiClient().getModelById_GetModel(id);
    return {
      model: response as any as Model
    };
  }

  static async deleteModel(id: string): Promise<void> {
    await ApiClientFactory.getApiClient().deleteModel_DeleteModel(id);
  }

  static async updateModel(id: string, modelData: Partial<Model>): Promise<void> {
    const request = new UpdateModelRequest({
      name: modelData.name,
      description: modelData.description,
      license: modelData.license as any,
      privacy: modelData.privacy as any,
      aiGenerated: modelData.aiGenerated,
      wip: modelData.wip,
      nsfw: modelData.nsfw,
      isRemix: modelData.isRemix,
      remixUrl: modelData.remixUrl,
      isFeatured: modelData.isFeatured
    });
    await ApiClientFactory.getApiClient().updateModel_UpdateModel(id, request);
  }

  static async createModelVersion(modelId: string, versionData: { name: string; files: File[] }): Promise<void> {
    const fileParameters: FileParameter[] = versionData.files.map(file => ({
      data: file,
      fileName: file.name
    }));
    await ApiClientFactory.getApiClient().createModelVersion_CreateModelVersion(
      modelId,
      fileParameters,
      versionData.name,
      null,
      undefined
    );
  }

  static async getModelVersions(modelId: string): Promise<any> {
    const response = await ApiClientFactory.getApiClient().getModelVersions_GetModelVersions(modelId);
    if (response && (response as any).data instanceof Blob) {
      const text = await (response as any).data.text();
      return text ? JSON.parse(text) : response;
    }
    return response;
  }

  // Dashboard methods for different model categories
  static async getFeaturedModels(): Promise<ExtendedModel[]> {
    try {
      const response = await ApiClientFactory.getApiClient().getModels_GetModels(1, 50);
      
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
      const response = await ApiClientFactory.getApiClient().getModels_GetModels(1, 50);
      
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
      const response = await ApiClientFactory.getApiClient().getModels_GetModels(1, 50);
      
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