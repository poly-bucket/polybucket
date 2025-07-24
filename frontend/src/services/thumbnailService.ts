import { API_CONFIG } from '../api/config';

const getAuthToken = (): string | null => {
  return localStorage.getItem('accessToken');
};

export interface ThumbnailGenerationSettings {
  backgroundColor: string;
  modelColor: string;
  metalness: number;
  roughness: number;
  autoRotate: boolean;
  cameraDistance: number;
  lightIntensity: number;
  lightColor: string;
  viewMode: 'solid' | 'wireframe' | 'points';
}

export interface ThumbnailGenerationRequest {
  modelFileUrl: string;
  fileType: string;
  size?: string;
  settings: ThumbnailGenerationSettings;
  forceRegenerate?: boolean;
}

export interface ThumbnailGenerationResponse {
  modelId: string;
  size: string;
  previewUrl: string;
  status: string;
  message: string;
  isQueued: boolean;
  queuedAt: string;
}

export class ThumbnailService {
  static async generateCustomThumbnail(
    modelId: string,
    request: ThumbnailGenerationRequest
  ): Promise<ThumbnailGenerationResponse> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const response = await fetch(`${API_CONFIG.baseUrl}/api/models/${modelId}/custom-thumbnail`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Thumbnail generation failed: ${response.status} ${response.statusText} - ${errorText}`);
    }

    return await response.json();
  }

  static async generateAutomaticThumbnail(modelId: string): Promise<ThumbnailGenerationResponse> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const response = await fetch(`${API_CONFIG.baseUrl}/api/models/${modelId}/previews`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        size: 'thumbnail',
        forceRegenerate: false
      })
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Automatic thumbnail generation failed: ${response.status} ${response.statusText} - ${errorText}`);
    }

    return await response.json();
  }

  static async getThumbnailStatus(modelId: string, size: string = 'thumbnail'): Promise<ThumbnailGenerationResponse> {
    const token = getAuthToken();
    if (!token) {
      throw new Error('Authentication token not found');
    }

    const response = await fetch(`${API_CONFIG.baseUrl}/api/models/${modelId}/previews/${size}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to get thumbnail status: ${response.status} ${response.statusText} - ${errorText}`);
    }

    return await response.json();
  }
} 