import { API_CONFIG } from '../api/config';

export interface DeleteModelResponse {
  success: boolean;
  message?: string;
}

export class DeleteModelService {
  private static baseUrl = API_CONFIG.baseUrl;

  static async deleteModel(modelId: string, accessToken: string): Promise<DeleteModelResponse> {
    try {
      const response = await fetch(`${this.baseUrl}/api/models/${modelId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        return { success: true };
      } else {
        const errorData = await response.json().catch(() => ({}));
        return { 
          success: false, 
          message: errorData.message || `Delete failed: ${response.statusText}` 
        };
      }
    } catch (error) {
      console.error('Failed to delete model:', error);
      return { 
        success: false, 
        message: error instanceof Error ? error.message : 'Unknown error occurred' 
      };
    }
  }

  static async deleteModels(modelIds: string[], accessToken: string): Promise<DeleteModelResponse> {
    try {
      const deletePromises = modelIds.map(id => this.deleteModel(id, accessToken));
      const results = await Promise.all(deletePromises);
      
      const failedDeletes = results.filter(result => !result.success);
      
      if (failedDeletes.length === 0) {
        return { success: true };
      } else if (failedDeletes.length === results.length) {
        return { 
          success: false, 
          message: 'All deletions failed' 
        };
      } else {
        return { 
          success: true, 
          message: `${results.length - failedDeletes.length} of ${results.length} models deleted successfully` 
        };
      }
    } catch (error) {
      console.error('Failed to delete models:', error);
      return { 
        success: false, 
        message: error instanceof Error ? error.message : 'Unknown error occurred' 
      };
    }
  }
}
