import { 
  GetSiteModelSettingsClient, 
  UpdateSiteModelSettingsClient, 
  UpdateSiteModelSettingsCommand,
  GetSiteModelSettingsResponse,
  UpdateSiteModelSettingsResponse,
  SiteModelSettingsData
} from './api.client';

export type SiteModelSettings = SiteModelSettingsData;

class SiteModelSettingsService {
  private getSiteModelSettingsClient(): GetSiteModelSettingsClient {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
    return new GetSiteModelSettingsClient(baseUrl);
  }

  private getUpdateSiteModelSettingsClient(): UpdateSiteModelSettingsClient {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
    return new UpdateSiteModelSettingsClient(baseUrl);
  }

  async getSettings(): Promise<GetSiteModelSettingsResponse> {
    try {
      const client = this.getSiteModelSettingsClient();
      return await client.getSiteModelSettings();
    } catch (error) {
      console.error('Error fetching site model settings:', error);
      throw error;
    }
  }

  async updateSettings(settings: SiteModelSettings): Promise<UpdateSiteModelSettingsResponse> {
    try {
      const client = this.getUpdateSiteModelSettingsClient();
      const command = new UpdateSiteModelSettingsCommand();
      
      // Map the settings to the command
      command.maxFileSizeBytes = settings.maxFileSizeBytes;
      command.allowedFileTypes = settings.allowedFileTypes;
      command.maxFilesPerUpload = settings.maxFilesPerUpload;
      command.enableFileCompression = settings.enableFileCompression;
      command.autoGeneratePreviews = settings.autoGeneratePreviews;
      command.defaultModelPrivacy = settings.defaultModelPrivacy;
      command.autoApproveModels = settings.autoApproveModels;
      command.requireModeration = settings.requireModeration;
      command.requireLoginForUpload = settings.requireLoginForUpload;
      command.allowPublicBrowsing = settings.allowPublicBrowsing;

      return await client.updateSiteModelSettings(command);
    } catch (error) {
      console.error('Error updating site model settings:', error);
      throw error;
    }
  }
}

const siteModelSettingsService = new SiteModelSettingsService();
export default siteModelSettingsService;
