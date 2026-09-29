import { API_CONFIG } from '../api/config';
import { ApiClientFactory } from '../api/clientFactory';

export interface LikeModelResult {
  success: boolean;
  message?: string;
}

export class LikeModelService {
  static async likeModel(modelId: string): Promise<LikeModelResult> {
    try {
      await ApiClientFactory.getApiClient().likeModel_LikeModel(modelId);
      return { success: true };
    } catch (error) {
      console.error('Failed to like model:', error);
      return {
        success: false,
        message: error instanceof Error ? error.message : 'Failed to like model',
      };
    }
  }

  static async unlikeModel(modelId: string, accessToken: string): Promise<LikeModelResult> {
    try {
      const response = await fetch(`${API_CONFIG.baseUrl}/api/models/${modelId}/like`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.ok) {
        return { success: true };
      }

      const errorData = await response.json().catch(() => ({}));
      return {
        success: false,
        message: (errorData as { message?: string }).message || `Unlike failed: ${response.statusText}`,
      };
    } catch (error) {
      console.error('Failed to unlike model:', error);
      return {
        success: false,
        message: error instanceof Error ? error.message : 'Failed to unlike model',
      };
    }
  }

  static async toggleLike(
    modelId: string,
    currentlyLiked: boolean,
    accessToken?: string
  ): Promise<LikeModelResult> {
    if (!accessToken) {
      return { success: false, message: 'Authentication required' };
    }

    if (currentlyLiked) {
      return this.unlikeModel(modelId, accessToken);
    }

    return this.likeModel(modelId);
  }
}
