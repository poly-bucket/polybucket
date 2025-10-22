import { ApiClient, DeleteAllModelsRequest, DeleteAllModelsResponse } from '../api/client';
import { API_CONFIG } from '../api/config';

export interface DeleteAllModelsServiceRequest {
  adminPassword: string;
}

export interface DeleteAllModelsServiceResponse {
  success: boolean;
  message: string;
  deletedCount: number;
  deletedAt: string;
}

class DeleteAllModelsService {
  private apiClient: ApiClient;

  constructor() {
    this.apiClient = new ApiClient(API_CONFIG.baseUrl);
  }

  /**
   * Delete all models from the system (DANGEROUS OPERATION - Admin only)
   * @param request - Request containing admin password
   * @returns Promise with deletion results
   */
  async deleteAllModels(request: DeleteAllModelsServiceRequest): Promise<DeleteAllModelsServiceResponse> {
    try {
      const apiRequest: DeleteAllModelsRequest = new DeleteAllModelsRequest();
      apiRequest.adminPassword = request.adminPassword;

      const response: DeleteAllModelsResponse = await this.apiClient.deleteAllModels_DeleteAllModels(apiRequest);
      
      return {
        success: response.success || false,
        message: response.message || 'Unknown error',
        deletedCount: response.deletedCount || 0,
        deletedAt: response.deletedAt?.toString() || new Date().toISOString()
      };
    } catch (error: any) {
      console.error('Error deleting all models:', error);
      
      // Handle different types of errors
      if (error.response?.status === 401) {
        throw new Error('Unauthorized: Invalid admin password or insufficient permissions');
      } else if (error.response?.status === 403) {
        throw new Error('Forbidden: Only administrators can delete all models');
      } else if (error.response?.status === 400) {
        throw new Error('Bad Request: Invalid request parameters');
      } else if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
      } else {
        throw new Error('An unexpected error occurred while deleting all models');
      }
    }
  }
}

export const deleteAllModelsService = new DeleteAllModelsService();
