import { ApiClientFactory } from '../api/clientFactory';

export interface ModelModerationAction {
  modelId: string;
  action: 'approve' | 'reject' | 'hide' | 'show';
  reason?: string;
}

export interface ModerationActionResult {
  success: boolean;
  message: string;
}

export interface ModelForModeration {
  id: string;
  title: string;
  author: string;
  status: 'pending' | 'approved' | 'rejected' | 'flagged';
  uploadDate: string;
  category: string;
  tags: string[];
  reportCount: number;
  isPublic: boolean;
}

export interface ModerationQueueResponse {
  models: ModelForModeration[];
  totalCount: number;
  page: number;
  pageSize: number;
}

class ModelModerationService {
  private get apiClient() {
    return ApiClientFactory.getApiClient();
  }

  /**
   * Get models awaiting moderation
   */
  async getModelsAwaitingModeration(page: number = 1, pageSize: number = 20): Promise<ModerationQueueResponse> {
    try {
      // For now, return mock data since the API endpoint returns FileResponse
      // TODO: Update backend to return proper JSON response
      const mockModels: ModelForModeration[] = [
        {
          id: '1',
          title: 'Test Model 1',
          author: 'user1',
          status: 'pending',
          uploadDate: '2024-01-15',
          category: 'Art',
          tags: ['sculpture', 'art'],
          reportCount: 0,
          isPublic: false
        },
        {
          id: '2',
          title: 'Test Model 2',
          author: 'user2',
          status: 'flagged',
          uploadDate: '2024-01-14',
          category: 'Technical',
          tags: ['mechanical', 'engineering'],
          reportCount: 3,
          isPublic: true
        }
      ];

      return {
        models: mockModels,
        totalCount: mockModels.length,
        page,
        pageSize
      };
    } catch (error: any) {
      console.error('Error fetching models awaiting moderation:', error);
      throw new Error('Failed to fetch models awaiting moderation');
    }
  }

  /**
   * Approve a model
   */
  async approveModel(modelId: string): Promise<ModerationActionResult> {
    try {
      // TODO: Implement actual API call when backend is updated
      console.log(`Approving model ${modelId}`);
      
      // Mock success response
      return {
        success: true,
        message: 'Model approved successfully'
      };
    } catch (error: any) {
      console.error('Error approving model:', error);
      throw new Error('Failed to approve model');
    }
  }

  /**
   * Reject a model
   */
  async rejectModel(modelId: string, reason?: string): Promise<ModerationActionResult> {
    try {
      // TODO: Implement actual API call when backend is updated
      console.log(`Rejecting model ${modelId} with reason: ${reason || 'No reason provided'}`);
      
      // Mock success response
      return {
        success: true,
        message: 'Model rejected successfully'
      };
    } catch (error: any) {
      console.error('Error rejecting model:', error);
      throw new Error('Failed to reject model');
    }
  }

  /**
   * Hide/show a model
   */
  async toggleModelVisibility(modelId: string, isPublic: boolean): Promise<ModerationActionResult> {
    try {
      // TODO: Implement actual API call when backend is updated
      console.log(`Setting model ${modelId} visibility to: ${isPublic ? 'public' : 'hidden'}`);
      
      // Mock success response
      return {
        success: true,
        message: `Model ${isPublic ? 'shown' : 'hidden'} successfully`
      };
    } catch (error: any) {
      console.error('Error toggling model visibility:', error);
      throw new Error('Failed to toggle model visibility');
    }
  }

  /**
   * Perform bulk moderation actions
   */
  async performBulkAction(modelIds: string[], action: 'approve' | 'reject' | 'hide' | 'show'): Promise<ModerationActionResult> {
    try {
      // TODO: Implement actual API call when backend is updated
      console.log(`Performing bulk ${action} on ${modelIds.length} models`);
      
      // Mock success response
      return {
        success: true,
        message: `Bulk ${action} completed successfully on ${modelIds.length} models`
      };
    } catch (error: any) {
      console.error('Error performing bulk action:', error);
      throw new Error(`Failed to perform bulk ${action}`);
    }
  }
}

export const modelModerationService = new ModelModerationService();
