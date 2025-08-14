import { apiClient } from './api.client';

export interface FileTypeSettingsData {
  id: string;
  fileExtension: string;
  enabled: boolean;
  maxFileSizeBytes: number;
  maxPerUpload: number;
  displayName: string;
  description: string;
  mimeType: string;
  requiresPreview: boolean;
  isCompressible: boolean;
  category: string;
  priority: number;
  isDefault: boolean;
}

export interface GetFileSettingsResponse {
  success: boolean;
  message: string;
  fileTypes?: FileTypeSettingsData[];
}

export interface UpdateFileSettingsRequest {
  id: string;
  fileExtension: string;
  enabled: boolean;
  maxFileSizeBytes: number;
  maxPerUpload: number;
  displayName: string;
  description: string;
  mimeType: string;
  requiresPreview: boolean;
  isCompressible: boolean;
  category: string;
  priority: number;
  isDefault: boolean;
}

export interface UpdateFileSettingsResponse {
  success: boolean;
  message: string;
}

class FileTypeSettingsService {
  async getFileSettings(): Promise<GetFileSettingsResponse> {
    try {
      const response = await apiClient.get('/api/system-settings/file-settings');
      return response.data;
    } catch (error) {
      console.error('Error fetching file type settings:', error);
      throw error;
    }
  }

  async updateFileSettings(request: UpdateFileSettingsRequest): Promise<UpdateFileSettingsResponse> {
    try {
      const response = await apiClient.put('/api/system-settings/file-settings', request);
      return response.data;
    } catch (error) {
      console.error('Error updating file type settings:', error);
      throw error;
    }
  }

  // Helper method to get enabled file types by category
  getEnabledFileTypesByCategory(fileTypes: FileTypeSettingsData[]): Record<string, FileTypeSettingsData[]> {
    const enabledTypes = fileTypes.filter(ft => ft.enabled);
    const grouped = enabledTypes.reduce((acc, fileType) => {
      if (!acc[fileType.category]) {
        acc[fileType.category] = [];
      }
      acc[fileType.category].push(fileType);
      return acc;
    }, {} as Record<string, FileTypeSettingsData[]>);

    // Sort each category by priority
    Object.keys(grouped).forEach(category => {
      grouped[category].sort((a, b) => a.priority - b.priority);
    });

    return grouped;
  }

  // Helper method to get all enabled file extensions
  getEnabledFileExtensions(fileTypes: FileTypeSettingsData[]): string[] {
    return fileTypes
      .filter(ft => ft.enabled)
      .map(ft => ft.fileExtension)
      .sort();
  }

  // Helper method to get max file size for a specific file type
  getMaxFileSizeForExtension(fileTypes: FileTypeSettingsData[], extension: string): number {
    const fileType = fileTypes.find(ft => 
      ft.enabled && ft.fileExtension.toLowerCase() === extension.toLowerCase()
    );
    return fileType?.maxFileSizeBytes || 100 * 1024 * 1024; // Default to 100MB
  }

  // Helper method to check if a file type is allowed
  isFileTypeAllowed(fileTypes: FileTypeSettingsData[], extension: string): boolean {
    return fileTypes.some(ft => 
      ft.enabled && ft.fileExtension.toLowerCase() === extension.toLowerCase()
    );
  }
}

export const fileTypeSettingsService = new FileTypeSettingsService();
export default fileTypeSettingsService;
