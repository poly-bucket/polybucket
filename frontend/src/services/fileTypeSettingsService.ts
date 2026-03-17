import { UpdateFileSettingsCommand } from '../api';
import { ApiClientFactory } from '../api/clientFactory';

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
      const client = ApiClientFactory.getApiClient();
      const response = await client.getFileSettings_GetFileSettings();
      return response as any as GetFileSettingsResponse;
    } catch (error) {
      console.error('Error fetching file type settings:', error);
      throw error;
    }
  }

  async updateFileSettings(request: UpdateFileSettingsRequest): Promise<UpdateFileSettingsResponse> {
    try {
      const client = ApiClientFactory.getApiClient();
      const updateCommand = new UpdateFileSettingsCommand(request);
      const response = await client.updateFileSettings_UpdateFileSettings(updateCommand);
      return response as any as UpdateFileSettingsResponse;
    } catch (error) {
      console.error('Error updating file type settings:', error);
      throw error;
    }
  }

  getEnabledFileTypesByCategory(fileTypes: FileTypeSettingsData[]): Record<string, FileTypeSettingsData[]> {
    const enabledTypes = fileTypes.filter(ft => ft.enabled);
    const grouped = enabledTypes.reduce((acc, fileType) => {
      if (!acc[fileType.category]) {
        acc[fileType.category] = [];
      }
      acc[fileType.category].push(fileType);
      return acc;
    }, {} as Record<string, FileTypeSettingsData[]>);

    Object.keys(grouped).forEach(category => {
      grouped[category].sort((a, b) => a.priority - b.priority);
    });

    return grouped;
  }

  getEnabledFileExtensions(fileTypes: FileTypeSettingsData[]): string[] {
    return fileTypes
      .filter(ft => ft.enabled)
      .map(ft => ft.fileExtension)
      .sort();
  }

  getMaxFileSizeForExtension(fileTypes: FileTypeSettingsData[], extension: string): number {
    const fileType = fileTypes.find(ft => 
      ft.enabled && ft.fileExtension.toLowerCase() === extension.toLowerCase()
    );
    return fileType?.maxFileSizeBytes || 100 * 1024 * 1024;
  }

  isFileTypeAllowed(fileTypes: FileTypeSettingsData[], extension: string): boolean {
    return fileTypes.some(ft => 
      ft.enabled && ft.fileExtension.toLowerCase() === extension.toLowerCase()
    );
  }
}

export const fileTypeSettingsService = new FileTypeSettingsService();
export default fileTypeSettingsService;
